-- 创建存储过程：获取专用设备分组分页数据
CREATE PROCEDURE [dbo].[GetSpecialEquipmentGroupedPaged]
    @Keyword NVARCHAR(255) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    
    -- 限制分页大小，防止查询过慢
    SET @PageSize = CASE WHEN @PageSize > 50 THEN 50 ELSE @PageSize END;
    
    -- 构建WHERE子句
    DECLARE @WhereClause NVARCHAR(MAX) = 'WHERE 1=1';
    
    IF @Keyword IS NOT NULL AND @Keyword != ''
    BEGIN
        SET @WhereClause += ' AND (DeviceName LIKE ''%' + @Keyword + '%'' OR Brand LIKE ''%' + @Keyword + '%'' OR Model LIKE ''%' + @Keyword + '%'')';
    END;
    
    -- 临时表存储分组信息
    CREATE TABLE #Groups (
        DeviceName NVARCHAR(255),
        Brand NVARCHAR(255),
        Model NVARCHAR(255)
    );
    
    -- 插入分组数据
    DECLARE @InsertGroupsSql NVARCHAR(MAX) = 'INSERT INTO #Groups
    SELECT DISTINCT DeviceName, Brand, Model
    FROM SpecialEquipment
    ' + @WhereClause + '
    ORDER BY DeviceName
    OFFSET ' + CAST((@PageNumber - 1) * @PageSize AS NVARCHAR) + ' ROWS FETCH NEXT ' + CAST(@PageSize AS NVARCHAR) + ' ROWS ONLY';
    
    EXEC sp_executesql @InsertGroupsSql;
    
    -- 统计分组数量
    DECLARE @GroupCount INT;
    DECLARE @CountSql NVARCHAR(MAX) = 'SELECT @GroupCount = COUNT(*) FROM (
        SELECT DISTINCT DeviceName, Brand, Model
        FROM SpecialEquipment
        ' + @WhereClause + '
    ) AS DistinctGroups';
    
    EXEC sp_executesql @CountSql, N'@GroupCount INT OUTPUT', @GroupCount OUTPUT;
    
    -- 统计设备总数
    DECLARE @DeviceCount INT;
    DECLARE @DeviceCountSql NVARCHAR(MAX) = 'SELECT @DeviceCount = COUNT(*) 
    FROM SpecialEquipment
    ' + @WhereClause;
    
    EXEC sp_executesql @DeviceCountSql, N'@DeviceCount INT OUTPUT', @DeviceCount OUTPUT;
    
    -- 查询分组内的所有设备
    SELECT e.*
    FROM SpecialEquipment e
    INNER JOIN #Groups g ON e.DeviceName = g.DeviceName 
        AND ISNULL(e.Brand, '') = ISNULL(g.Brand, '')
        AND ISNULL(e.Model, '') = ISNULL(g.Model, '')
    ORDER BY e.DeviceName, e.SortOrder, e.Id;
    
    -- 返回统计信息
    SELECT @GroupCount AS GroupCount, @DeviceCount AS DeviceCount;
    
    -- 清理临时表
    DROP TABLE #Groups;
END;
GO

-- 创建存储过程：获取专用设备可用分页数据
CREATE PROCEDURE [dbo].[GetSpecialEquipmentAvailablePaged]
    @Keyword NVARCHAR(255) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    
    -- 构建WHERE子句
    DECLARE @WhereClause NVARCHAR(MAX) = 'WHERE Quantity > 0 AND UseStatus != 1';
    
    IF @Keyword IS NOT NULL AND @Keyword != ''
    BEGIN
        SET @WhereClause += ' AND (DeviceName LIKE ''%' + @Keyword + '%'' OR Brand LIKE ''%' + @Keyword + '%'' OR Model LIKE ''%' + @Keyword + '%'')';
    END;
    
    -- 统计分组数量
    DECLARE @TotalCount INT;
    DECLARE @CountSql NVARCHAR(MAX) = 'SELECT @TotalCount = COUNT(*) FROM (
        SELECT DISTINCT DeviceName, Brand, Model
        FROM SpecialEquipment
        ' + @WhereClause + '
    ) AS DistinctItems';
    
    EXEC sp_executesql @CountSql, N'@TotalCount INT OUTPUT', @TotalCount OUTPUT;
    
    -- 查询数据
    DECLARE @DataSql NVARCHAR(MAX) = 'SELECT 
        DeviceName,
        Brand,
        Model,
        SUM(Quantity) AS Quantity,
        MAX(Unit) AS Unit,
        MAX(Location) AS Location,
        MAX(Company) AS Company,
        MAX(DeviceStatus) AS DeviceStatus
    FROM SpecialEquipment
    ' + @WhereClause + '
    GROUP BY DeviceName, Brand, Model
    ORDER BY DeviceName
    OFFSET ' + CAST((@PageNumber - 1) * @PageSize AS NVARCHAR) + ' ROWS FETCH NEXT ' + CAST(@PageSize AS NVARCHAR) + ' ROWS ONLY';
    
    EXEC sp_executesql @DataSql;
    
    -- 返回总数
    SELECT @TotalCount AS TotalCount;
END;
GO

-- 创建存储过程：获取通用设备可用分页数据
CREATE PROCEDURE [dbo].[GetGeneralEquipmentAvailablePaged]
    @Keyword NVARCHAR(255) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    
    -- 构建WHERE子句
    DECLARE @WhereClause NVARCHAR(MAX) = 'WHERE Quantity > 0 AND UseStatus != 1';
    
    IF @Keyword IS NOT NULL AND @Keyword != ''
    BEGIN
        SET @WhereClause += ' AND (DeviceName LIKE ''%' + @Keyword + '%'' OR Brand LIKE ''%' + @Keyword + '%'' OR Model LIKE ''%' + @Keyword + '%'')';
    END;
    
    -- 统计分组数量
    DECLARE @TotalCount INT;
    DECLARE @CountSql NVARCHAR(MAX) = 'SELECT @TotalCount = COUNT(*) FROM (
        SELECT DISTINCT DeviceName, Brand, Model
        FROM GeneralEquipment
        ' + @WhereClause + '
    ) AS DistinctItems';
    
    EXEC sp_executesql @CountSql, N'@TotalCount INT OUTPUT', @TotalCount OUTPUT;
    
    -- 查询数据
    DECLARE @DataSql NVARCHAR(MAX) = 'SELECT 
        DeviceName AS Name, 
        Brand, 
        Model, 
        SUM(Quantity) AS AvailableQuantity, 
        MAX(Unit) AS Unit, 
        MAX(Location) AS Location, 
        MAX(Company) AS Company,
        MAX(DeviceStatus) AS DeviceStatus
    FROM GeneralEquipment
    ' + @WhereClause + '
    GROUP BY DeviceName, Brand, Model
    ORDER BY DeviceName
    OFFSET ' + CAST((@PageNumber - 1) * @PageSize AS NVARCHAR) + ' ROWS FETCH NEXT ' + CAST(@PageSize AS NVARCHAR) + ' ROWS ONLY';
    
    EXEC sp_executesql @DataSql;
    
    -- 返回总数
    SELECT @TotalCount AS TotalCount;
END;
GO

-- 创建存储过程：获取耗材可用分页数据
CREATE PROCEDURE [dbo].[GetConsumableAvailablePaged]
    @Keyword NVARCHAR(255) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    
    -- 构建WHERE子句
    DECLARE @WhereClause NVARCHAR(MAX) = 'WHERE RemainingQuantity > 0';
    
    IF @Keyword IS NOT NULL AND @Keyword != ''
    BEGIN
        SET @WhereClause += ' AND (Name LIKE ''%' + @Keyword + '%'' OR Brand LIKE ''%' + @Keyword + '%'' OR ModelSpecification LIKE ''%' + @Keyword + '%'')';
    END;
    
    -- 统计分组数量
    DECLARE @TotalCount INT;
    DECLARE @CountSql NVARCHAR(MAX) = 'SELECT @TotalCount = COUNT(*) FROM (
        SELECT DISTINCT Name, Brand, ModelSpecification
        FROM Consumables
        ' + @WhereClause + '
    ) AS DistinctItems';
    
    EXEC sp_executesql @CountSql, N'@TotalCount INT OUTPUT', @TotalCount OUTPUT;
    
    -- 查询数据
    DECLARE @DataSql NVARCHAR(MAX) = 'SELECT 
        Name, 
        Brand, 
        ModelSpecification AS Model, 
        SUM(RemainingQuantity) AS AvailableQuantity, 
        MAX(Unit) AS Unit, 
        MAX(Location) AS Location, 
        MAX(Company) AS Company
    FROM Consumables
    ' + @WhereClause + '
    GROUP BY Name, Brand, ModelSpecification
    ORDER BY Name
    OFFSET ' + CAST((@PageNumber - 1) * @PageSize AS NVARCHAR) + ' ROWS FETCH NEXT ' + CAST(@PageSize AS NVARCHAR) + ' ROWS ONLY';
    
    EXEC sp_executesql @DataSql;
    
    -- 返回总数
    SELECT @TotalCount AS TotalCount;
END;
GO