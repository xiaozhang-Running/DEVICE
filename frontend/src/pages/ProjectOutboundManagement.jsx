import React, { useState, useEffect, useCallback } from 'react';
import { Card, Table, Button, Input, Select, Space, message, Modal, Form, DatePicker, Upload, Image, Tag, Pagination } from 'antd';
import { SearchOutlined, PlusOutlined, EditOutlined, DeleteOutlined, EyeOutlined, UploadOutlined, ReloadOutlined } from '@ant-design/icons';
import usePermission from '../hooks/usePermission';
import projectOutboundApi from '../api/projectOutbound';
import { debounce } from '../utils/cache';
import dayjs from 'dayjs';

const { Option } = Select;
const { Search } = Input;
const { RangePicker } = DatePicker;

const ProjectOutboundManagement = () => {
  const [projectOutbounds, setProjectOutbounds] = useState([]);
  const [loading, setLoading] = useState(false);
  const [searchKeyword, setSearchKeyword] = useState('');
  const [filters, setFilters] = useState({
    outboundType: null,
    status: null,
    inboundStatus: null
  });
  const [modalVisible, setModalVisible] = useState(false);
  const [previewModalVisible, setPreviewModalVisible] = useState(false);
  const [selectItemModalVisible, setSelectItemModalVisible] = useState(false);
  const [itemDetailModalVisible, setItemDetailModalVisible] = useState(false);
  const [editingOutbound, setEditingOutbound] = useState(null);
  const [previewOutbound, setPreviewOutbound] = useState(null);
  const [form] = Form.useForm();
  const [availableItems, setAvailableItems] = useState([]);
  const [selectedItems, setSelectedItems] = useState([]);
  const [outboundImages, setOutboundImages] = useState([]);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [total, setTotal] = useState(0);
  const [itemPage, setItemPage] = useState(1);
  const [itemTotal, setItemTotal] = useState(0);
  const [itemKeyword, setItemKeyword] = useState('');
  const [selectedItemType, setSelectedItemType] = useState(1);
  const [currentItem, setCurrentItem] = useState(null);
  const [itemDetailList, setItemDetailList] = useState([]);
  const [itemDetailKeyword, setItemDetailKeyword] = useState('');
  const { canAction } = usePermission();

  const fetchProjectOutbounds = async (keyword = '', filterParams = {}) => {
    setLoading(true);
    try {
      let response;
      if (keyword) {
        response = await projectOutboundApi.searchProjectOutbounds(keyword);
      } else {
        response = await projectOutboundApi.getProjectOutbounds(filterParams);
      }
      
      if (response.success && response.data) {
        const outbounds = Array.isArray(response.data) ? response.data : response.data.items || [];
        setProjectOutbounds(outbounds);
        setTotal(outbounds.length);
      }
    } catch (error) {
      message.error('获取项目出库列表失败');
      console.error('Error fetching project outbounds:', error);
    } finally {
      setLoading(false);
    }
  };

  const fetchAvailableItems = async (keyword = '', page = 1, itemType = null) => {
    setLoading(true);
    try {
      console.log('Fetching available items with:', { keyword, page, itemType });
      const response = await projectOutboundApi.getAvailableItemsPaged({
        Keyword: keyword,
        PageNumber: page,
        PageSize: 10,
        ItemType: itemType
      });
      
      if (response.success && response.data) {
        // 为每个物品生成唯一ID，避免key重复
        const itemsWithUniqueId = (response.data.items || []).map((item, index) => ({
          ...item,
          id: item.id || `item-${index}-${Date.now()}`, // 生成唯一ID
          availableQuantity: item.availableQuantity || item.AvailableQuantity || 1 // 确保availableQuantity存在
        }));
        setAvailableItems(itemsWithUniqueId);
        setItemTotal(response.data.totalCount || 0);
      }
    } catch (error) {
      message.error('获取可用物品失败');
      console.error('Error fetching available items:', error);
    } finally {
      setLoading(false);
    }
  };

  const fetchItemDetailsByType = async (item) => {
    setLoading(true);
    try {
      // 根据物品类型获取详细信息
      if (item.itemType === 3 || item.itemType === 'consumable') {
        // 耗材类型，只返回一条记录，让用户输入数量
        // 检查当前已选物品中是否存在该耗材
        const existingConsumable = selectedItems.find(i => 
          (i.itemType === 3 || i.itemType === 'consumable') &&
          i.name === item.name &&
          i.brand === (item.brand || '-') &&
          i.model === (item.model || '-')
        );
        
        // 使用物品的原始ID作为itemId（应该是数字）
        const itemId = item.id || 0;
        const consumableDetail = {
          id: itemId, // 使用原始ID
          itemId: itemId, // 使用数字ID作为itemId
          itemName: item.name,
          deviceCode: item.name,
          brand: item.brand || '-',
          model: item.model || '-',
          quantity: existingConsumable ? existingConsumable.quantity : item.availableQuantity, // 如果已存在，使用已选数量，否则使用可用数量
          unit: item.unit || '个',
          deviceStatus: '正常',
          accessories: '-',
          remark: '-',
          itemTypeName: item.itemTypeName, // 保存物品类型名称
          itemType: item.itemType, // 保存物品类型
          availableQuantity: item.availableQuantity // 保存可用数量
        };
        setItemDetailList([consumableDetail]);
      } else if (item.itemType === 4 || item.itemType === 'rawMaterial') {
        // 原材料类型，只返回一条记录，让用户输入数量
        // 检查当前已选物品中是否存在该原材料
        const existingRawMaterial = selectedItems.find(i => 
          (i.itemType === 4 || i.itemType === 'rawMaterial') &&
          i.name === item.name &&
          i.brand === (item.brand || '-') &&
          i.model === (item.model || '-')
        );
        
        // 使用物品的原始ID作为itemId（应该是数字）
        const itemId = item.id || 0;
        const rawMaterialDetail = {
          id: itemId, // 使用原始ID
          itemId: itemId, // 使用数字ID作为itemId
          itemName: item.name,
          deviceCode: item.name,
          brand: item.brand || '-',
          model: item.model || '-',
          quantity: existingRawMaterial ? existingRawMaterial.quantity : item.availableQuantity, // 如果已存在，使用已选数量，否则使用可用数量
          unit: item.unit || '个',
          deviceStatus: '正常',
          accessories: '-',
          remark: '-',
          itemTypeName: item.itemTypeName, // 保存物品类型名称
          itemType: item.itemType, // 保存物品类型
          availableQuantity: item.availableQuantity // 保存可用数量
        };
        setItemDetailList([rawMaterialDetail]);
      } else {
        // 设备类型，调用详细API获取
        const response = await projectOutboundApi.getAvailableItems(item.name);
        if (response.success && response.data) {
          // 过滤出当前物品的详细信息
          const filteredItems = response.data.filter(detail => 
            detail.name === item.name && 
            detail.brand === item.brand && 
            detail.model === item.model
          );
          setItemDetailList(filteredItems.map((detail, idx) => ({
            id: detail.id, // 使用原始ID
            itemId: detail.id,
            itemName: detail.name,
            deviceCode: detail.deviceCode,
            brand: detail.brand || '-',
            model: detail.model || '-',
            quantity: 1,
            unit: detail.unit || '个',
            deviceStatus: detail.deviceStatus || '正常',
            accessories: detail.accessories || '-',
            remark: detail.remark || '-',
            itemTypeName: item.itemTypeName, // 保存物品类型名称
            itemType: item.itemType, // 保存物品类型
            availableQuantity: detail.availableQuantity || item.availableQuantity || 1 // 保存可用数量
          })));
        }
      }
      setCurrentItem(item);
      setItemDetailModalVisible(true);
    } catch (error) {
      message.error('获取物品详情失败');
      console.error('Error fetching item details:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchProjectOutbounds(searchKeyword, filters);
    // 默认加载专用设备
    fetchAvailableItems('', 1, 1);
  }, []);

  const handleSearch = useCallback(debounce((value) => {
    setSearchKeyword(value);
    fetchProjectOutbounds(value, filters);
  }, 300), [filters]);

  const handleFilter = () => {
    fetchProjectOutbounds(searchKeyword, filters);
  };

  const handleResetFilters = () => {
    setFilters({
      outboundType: null,
      status: null,
      inboundStatus: null
    });
    setSearchKeyword('');
    fetchProjectOutbounds('', {});
  };

  const generateOutboundNumber = () => {
    const date = new Date();
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const random = Math.floor(Math.random() * 10000).toString().padStart(4, '0');
    return `XMKC${year}${month}${day}${random}`;
  };

  const handleAddOutbound = () => {
    setEditingOutbound(null);
    setSelectedItems([]);
    setOutboundImages([]);
    form.resetFields();
    form.setFieldsValue({
      outboundNumber: generateOutboundNumber(),
      outboundDate: dayjs(),
      outboundType: '元动自用',
      logisticsMethod: 1
    });
    setModalVisible(true);
  };

  const handleEditOutbound = (outbound) => {
    setEditingOutbound(outbound);
    setSelectedItems(outbound.items || []);
    setOutboundImages(outbound.OutboundImages || []);
    form.setFieldsValue({
      ...outbound,
      outboundDate: outbound.outboundDate ? dayjs(outbound.outboundDate) : dayjs(),
      returnDate: outbound.returnDate ? dayjs(outbound.returnDate) : null
    });
    setModalVisible(true);
  };

  const handleDeleteOutbound = async (id) => {
    try {
      await projectOutboundApi.deleteProjectOutbound(id);
      setProjectOutbounds(projectOutbounds.filter(item => item.id !== id));
      setTotal(total - 1);
      message.success('项目出库删除成功');
    } catch (error) {
      message.error('项目出库删除失败');
      console.error('Error deleting project outbound:', error);
    }
  };

  const handleCompleteOutbound = async (id) => {
    try {
      const response = await projectOutboundApi.completeProjectOutbound(id);
      console.log('Complete response:', response);
      // 重新获取项目出库列表，确保设备状态被正确更新
      await fetchProjectOutbounds(searchKeyword, filters);
      message.success(response?.message || '项目出库已完成');
    } catch (error) {
      message.error(error?.response?.data?.message || '操作失败');
      console.error('Error completing project outbound:', error);
    }
  };

  const handleSubmit = async (values) => {
    try {
      // 映射 itemType 为数字类型
      const getItemTypeNumber = (itemType) => {
        if (itemType === 'consumable' || itemType === 3) return 3;
        if (itemType === 'specialEquipment' || itemType === 1) return 1;
        if (itemType === 'generalEquipment' || itemType === 2) return 2;
        if (itemType === 'rawMaterial' || itemType === 4) return 4;
        return itemType;
      };
      
      const data = {
        OutboundNumber: values.outboundNumber,
        OutboundDate: values.outboundDate ? values.outboundDate.format('YYYY-MM-DD') : null,
        ProjectName: values.projectName,
        ProjectManager: values.projectManager,
        Recipient: values.recipient,
        OutboundType: values.outboundType,
        ProjectTime: values.projectTime,
        ContactPhone: values.contactPhone,
        UsageLocation: values.usageLocation,
        ReturnDate: values.returnDate ? values.returnDate.format('YYYY-MM-DD') : null,
        Handler: values.handler,
        WarehouseKeeper: values.warehouseKeeper,
        LogisticsMethod: values.logisticsMethod,
        OutboundImages: outboundImages,
        Remark: values.remark,
        Items: selectedItems.map(item => ({
          ItemType: getItemTypeNumber(item.itemType),
          ItemId: item.itemId || item.id, // 使用物品的实际ID
          ItemName: item.name,
          DeviceCode: item.deviceCode,
          Brand: item.brand,
          Model: item.model,
          Quantity: item.quantity,
          Unit: item.unit,
          Accessories: item.accessories,
          Remark: item.remark,
          DeviceStatus: item.deviceStatus
        }))
      };

      if (editingOutbound) {
        await projectOutboundApi.updateProjectOutbound(editingOutbound.id, data);
        setProjectOutbounds(projectOutbounds.map(item => 
          item.id === editingOutbound.id ? { ...item, ...data } : item
        ));
        message.success('项目出库更新成功');
      } else {
        const response = await projectOutboundApi.createProjectOutbound(data);
        const newOutbound = response.data;
        setProjectOutbounds([newOutbound, ...projectOutbounds]);
        setTotal(total + 1);
        message.success('项目出库创建成功');
      }
      setModalVisible(false);
    } catch (error) {
      message.error('操作失败');
      console.error('Error submitting form:', error);
    }
  };

  const fetchItemDetails = async (item) => {
    setLoading(true);
    try {
      // 这里应该调用后端API获取具体物品的详情列表
      // 暂时使用模拟数据
      const mockDetails = Array.from({ length: item.availableQuantity }, (_, index) => ({
        id: `${item.id}-${index + 1}`,
        itemId: item.id,
        itemName: item.name,
        deviceCode: item.itemType === 3 ? `${item.name}-${String(index + 1).padStart(3, '0')}` : `${item.deviceCode || 'YD-马拉松地毯'}-${String(index + 1).padStart(3, '0')}`,
        brand: item.brand || '-',
        model: item.model || '-',
        quantity: 1,
        unit: item.unit || (item.itemType === 3 ? '个' : '个'),
        deviceStatus: item.itemType === 3 ? '正常' : (item.deviceStatus || '正常'),
        accessories: '-',
        remark: '-'
      }));
      setItemDetailList(mockDetails);
      setCurrentItem(item);
      setItemDetailModalVisible(true);
    } catch (error) {
      message.error('获取物品详情失败');
      console.error('Error fetching item details:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleSelectItem = (item) => {
    // 打开物品详情模态框
    console.log('Selected item:', item); // 调试：查看 item 对象的结构
    fetchItemDetailsByType(item);
  };

  const handleAddItemDetail = (detail) => {
    // 对于耗材和原材料，使用物品名称、品牌和型号来查找已存在的物品
    let existingItemIndex;
    if (detail.itemType === 3 || detail.itemType === 4) {
      existingItemIndex = selectedItems.findIndex(i => 
        (i.itemType === 3 || i.itemType === 4) &&
        i.name === detail.itemName &&
        (i.brand === (detail.brand || '-') || i.brand === detail.brand) &&
        (i.model === (detail.model || '-') || i.model === detail.model)
      );
    } else {
      // 对于设备，使用ID来查找
      existingItemIndex = selectedItems.findIndex(i => i.id === detail.id);
    }
    
    if (existingItemIndex === -1) {
      // 物品不存在，添加新物品
      // 对于耗材和原材料，使用detail.id作为itemId（应该是数字）
      const itemId = detail.id || 0;
      
      setSelectedItems([...selectedItems, {
        ...detail,
        id: detail.id, // 使用原始ID
        itemId: itemId, // 使用数字ID作为itemId
        name: detail.itemName || detail.name,
        itemTypeName: detail.itemTypeName || currentItem.itemTypeName,
        itemType: detail.itemType || currentItem.itemType,
        quantity: detail.quantity || 1 // 确保数量存在
      }]);
      message.success('添加成功');
    } else {
      // 物品已存在，更新数量
      const updatedItems = [...selectedItems];
      updatedItems[existingItemIndex] = {
        ...updatedItems[existingItemIndex],
        quantity: detail.quantity || 1 // 确保数量存在
      };
      setSelectedItems(updatedItems);
      message.success('数量更新成功');
    }
  };

  const handleRemoveItem = (itemId) => {
    setSelectedItems(selectedItems.filter(item => item.id !== itemId));
  };

  const handlePreviewOutbound = (outbound) => {
    setPreviewOutbound(outbound);
    setPreviewModalVisible(true);
  };

  const handlePreview = () => {
    setPreviewOutbound({
      ...form.getFieldsValue(),
      items: selectedItems
    });
    setPreviewModalVisible(true);
  };

  const columns = [
    {
      title: '出库单号',
      dataIndex: 'outboundNumber',
      key: 'outboundNumber',
      width: 150
    },
    {
      title: '出库日期',
      dataIndex: 'outboundDate',
      key: 'outboundDate',
      width: 120,
      render: (date) => date ? new Date(date).toLocaleDateString() : ''
    },
    {
      title: '出库类型',
      dataIndex: 'outboundType',
      key: 'outboundType',
      width: 100
    },
    {
      title: '项目名称',
      dataIndex: 'projectName',
      key: 'projectName',
      width: 150
    },
    {
      title: '项目负责人',
      dataIndex: 'projectManager',
      key: 'projectManager',
      width: 120
    },
    {
      title: '使用地',
      dataIndex: 'usageLocation',
      key: 'usageLocation',
      width: 100
    },
    {
      title: '领用人',
      dataIndex: 'recipient',
      key: 'recipient',
      width: 100
    },
    {
      title: '经手人',
      dataIndex: 'handler',
      key: 'handler',
      width: 100
    },
    {
      title: '库管',
      dataIndex: 'warehouseKeeper',
      key: 'warehouseKeeper',
      width: 100
    },
    {
      title: '预计归还日期',
      dataIndex: 'returnDate',
      key: 'returnDate',
      width: 120,
      render: (date) => date ? new Date(date).toLocaleDateString() : ''
    },
    {
      title: '总数量',
      dataIndex: 'totalQuantity',
      key: 'totalQuantity',
      width: 80
    },
    {
      title: '状态',
      dataIndex: 'isCompleted',
      key: 'isCompleted',
      width: 80,
      render: (completed) => (
        <Tag color={completed ? 'green' : 'orange'}>{completed ? '已完成' : '进行中'}</Tag>
      )
    },
    {
      title: '入库状态',
      dataIndex: 'inboundStatus',
      key: 'inboundStatus',
      width: 100,
      render: (status) => {
        let color = 'blue';
        let text = '未入库';
        if (status === '部分入库') color = 'orange';
        else if (status === '全入库') color = 'green';
        return <Tag color={color}>{status || text}</Tag>;
      }
    },
    {
      title: '操作',
      key: 'action',
      width: 200,
      fixed: 'right',
      render: (_, record) => (
        <Space size="small">
          <Button 
            icon={<EyeOutlined />} 
            onClick={() => handlePreviewOutbound(record)}
          >
            预览
          </Button>
          <Button 
            type="primary" 
            icon={<EditOutlined />} 
            onClick={() => handleEditOutbound(record)}
          >
            编辑
          </Button>
          <Button 
            type="primary" 
            onClick={() => handleCompleteOutbound(record.id)}
          >
            完成
          </Button>
          <Button 
            danger 
            icon={<DeleteOutlined />}
            onClick={() => handleDeleteOutbound(record.id)}
          >
            删除
          </Button>
        </Space>
      )
    }
  ];

  return (
    <div className="project-outbound-management">
      <Card
        title="项目出库管理"
        extra={
          <Space>
            <Search
              placeholder="搜索产品名称/规格/供应商..."
              value={searchKeyword}
              onChange={(e) => setSearchKeyword(e.target.value)}
              onSearch={handleSearch}
              style={{ width: 300 }}
              allowClear
            />
            <Select
              placeholder="出库类型"
              value={filters.outboundType}
              onChange={(value) => setFilters({ ...filters, outboundType: value })}
              style={{ width: 120 }}
              allowClear
            >
              <Option value="元动自用">元动自用</Option>
              <Option value="对外租借">对外租借</Option>
              <Option value="其他">其他</Option>
            </Select>
            <Select
              placeholder="状态"
              value={filters.status}
              onChange={(value) => setFilters({ ...filters, status: value })}
              style={{ width: 120 }}
              allowClear
            >
              <Option value="true">已完成</Option>
              <Option value="false">进行中</Option>
            </Select>
            <Select
              placeholder="入库状态"
              value={filters.inboundStatus}
              onChange={(value) => setFilters({ ...filters, inboundStatus: value })}
              style={{ width: 120 }}
              allowClear
            >
              <Option value="未入库">未入库</Option>
              <Option value="部分入库">部分入库</Option>
              <Option value="全入库">全入库</Option>
            </Select>
            <Button type="primary" icon={<ReloadOutlined />} onClick={handleResetFilters}>
              重置
            </Button>
            <Button type="primary" icon={<SearchOutlined />} onClick={handleFilter}>
              筛选
            </Button>
            {canAction('create') && (
              <Button type="primary" icon={<PlusOutlined />} onClick={handleAddOutbound}>
              新建项目出库
              </Button>
            )}
          </Space>
        }
      >
        <Table
          columns={columns}
          dataSource={projectOutbounds}
          rowKey={(record) => record.Id || record.id || Math.random().toString(36)}
          loading={loading}
          pagination={{
            current: currentPage,
            pageSize: pageSize,
            total: total,
            onChange: (page, size) => {
              setCurrentPage(page);
              setPageSize(size);
            },
            showSizeChanger: true,
            showQuickJumper: true,
            showTotal: (total) => `共 ${total} 条`
          }}
          scroll={{ x: 1600 }}
        />
      </Card>

      <Modal
        title={editingOutbound ? '编辑项目出库' : '新建项目出库单'}
        open={modalVisible}
        onCancel={() => setModalVisible(false)}
        footer={null}
        width={1200}
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleSubmit}
        >
          <div style={{ display: 'flex', gap: 16, marginBottom: 16 }}>
            <Form.Item
              name="outboundNumber"
              label="出库单号"
              style={{ flex: 1.5 }}
            >
              <Input placeholder="出库单号" disabled />
            </Form.Item>
            <Form.Item
              name="outboundType"
              label="出库类型"
              rules={[{ required: true, message: '请选择出库类型' }]}
              style={{ flex: 1 }}
            >
              <Select placeholder="请选择出库类型">
                <Option value="元动自用">元动自用</Option>
                <Option value="对外租借">对外租借</Option>
                <Option value="其他">其他</Option>
              </Select>
            </Form.Item>
            <Form.Item
              name="logisticsMethod"
              label="物流方式"
              style={{ flex: 1 }}
            >
              <Select placeholder="请选择物流方式">
                <Option value={1}>随身携带</Option>
                <Option value={2}>跨越物流</Option>
                <Option value={3}>德邦物流</Option>
                <Option value={4}>顺丰物流</Option>
                <Option value={5}>其他</Option>
              </Select>
            </Form.Item>
            <Form.Item
              name="projectName"
              label="项目名称"
              rules={[{ required: true, message: '请输入项目名称' }]}
              style={{ flex: 1.5 }}
            >
              <Input placeholder="请输入项目名称" />
            </Form.Item>
            <Form.Item
              name="projectManager"
              label="项目负责人"
              rules={[{ required: true, message: '请输入项目负责人' }]}
              style={{ flex: 1 }}
            >
              <Input placeholder="请输入项目负责人" />
            </Form.Item>
          </div>

          <div style={{ display: 'flex', gap: 16, marginBottom: 16 }}>
            <Form.Item
              name="contactPhone"
              label="联系电话"
              rules={[{ required: true, message: '请输入联系电话' }]}
              style={{ flex: 1 }}
            >
              <Input placeholder="请输入联系电话" />
            </Form.Item>
            <Form.Item
              name="projectTime"
              label="项目时间"
              rules={[{ required: true, message: '请输入项目时间' }]}
              style={{ flex: 1.5 }}
            >
              <Input placeholder="请输入项目时间，如：2026.03.01-2026.03.15" />
            </Form.Item>
            <Form.Item
              name="returnDate"
              label="预计归还日期"
              style={{ flex: 1 }}
            >
              <DatePicker style={{ width: '100%' }} />
            </Form.Item>
            <Form.Item
              name="usageLocation"
              label="使用地"
              style={{ flex: 1 }}
            >
              <Input placeholder="请输入使用地" />
            </Form.Item>
          </div>

          <Form.Item label="出库图片" style={{ marginBottom: 16 }}>
            <div style={{ display: 'flex', gap: 16 }}>
              <div style={{ border: '1px dashed #d9d9d9', borderRadius: 4, width: 100, height: 100, display: 'flex', alignItems: 'center', justifyContent: 'center', cursor: 'pointer' }}
                   onClick={() => {
                     // 实现图片上传功能
                     const input = document.createElement('input');
                     input.type = 'file';
                     input.accept = 'image/*';
                     input.multiple = false;
                     input.onchange = (e) => {
                       const file = e.target.files[0];
                       if (file) {
                         // 创建图片预览URL
                         const imageUrl = URL.createObjectURL(file);
                         // 添加到图片列表
                         setOutboundImages([...outboundImages, imageUrl]);
                         message.success('图片选择成功');
                       }
                     };
                     input.click();
                   }}>
                <div style={{ textAlign: 'center' }}>
                  <PlusOutlined style={{ fontSize: 24, color: '#999' }} />
                  <div style={{ fontSize: 12, color: '#999', marginTop: 8 }}>上传图片</div>
                </div>
              </div>
              {outboundImages.map((imageUrl, index) => (
                <div key={index} style={{ position: 'relative' }}>
                  <img src={imageUrl} style={{ width: 100, height: 100, objectFit: 'cover', borderRadius: 4 }} />
                  <button
                    style={{
                      position: 'absolute',
                      top: -8,
                      right: -8,
                      width: 20,
                      height: 20,
                      borderRadius: '50%',
                      backgroundColor: 'red',
                      color: 'white',
                      border: 'none',
                      cursor: 'pointer',
                      fontSize: 12,
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center'
                    }}
                    onClick={() => {
                      const updatedImages = outboundImages.filter((_, i) => i !== index);
                      setOutboundImages(updatedImages);
                    }}
                  >
                    ×
                  </button>
                </div>
              ))}
            </div>
          </Form.Item>

          <div style={{ display: 'flex', gap: 16, marginBottom: 16 }}>
            <div style={{ flex: 1, border: '1px solid #f0f0f0', borderRadius: 4 }}>
              <div style={{ padding: 12, borderBottom: '1px solid #f0f0f0', fontWeight: 'bold' }}>
                已选物品 (共 {selectedItems.length} 项)
              </div>
              <div style={{ padding: 12, maxHeight: 300, overflow: 'auto' }}>
                {selectedItems.length > 0 ? (
                  <div>
                    <div style={{ display: 'flex', marginBottom: 8, fontWeight: 'bold', fontSize: 12 }}>
                      <div style={{ flex: 0.5 }}>序号</div>
                      <div style={{ flex: 1 }}>物品类型</div>
                      <div style={{ flex: 1.5 }}>物品名称</div>
                      <div style={{ flex: 1 }}>设备编号</div>
                      <div style={{ flex: 1 }}>品牌</div>
                      <div style={{ flex: 1 }}>型号</div>
                      <div style={{ flex: 0.8 }}>数量</div>
                      <div style={{ flex: 1 }}>设备状态</div>
                      <div style={{ flex: 0.5 }}>操作</div>
                    </div>
                    {selectedItems.map((item, index) => (
                      <div key={item.id} style={{ display: 'flex', marginBottom: 8, alignItems: 'center', fontSize: 12 }}>
                        <div style={{ flex: 0.5 }}>{index + 1}</div>
                        <div style={{ flex: 1 }}>
                          <Tag color={item.itemType === 1 ? 'blue' : item.itemType === 2 ? 'green' : item.itemType === 3 ? 'orange' : 'purple'}>
                            {item.itemTypeName}
                          </Tag>
                        </div>
                        <div style={{ flex: 1.5 }}>{item.name}</div>
                        <div style={{ flex: 1 }}>{item.deviceCode || '-'}</div>
                        <div style={{ flex: 1 }}>{item.brand || '-'}</div>
                        <div style={{ flex: 1 }}>{item.model || '-'}</div>
                        <div style={{ flex: 0.8 }}>
                          {(item.itemType === 3 || item.itemType === 'consumable') ? (
                            <Input 
                              type="number" 
                              min="1" 
                              max={item.availableQuantity} 
                              value={item.quantity} 
                              onChange={(e) => {
                                const newQuantity = parseInt(e.target.value) || 1;
                                // 确保数量不超过可用数量
                                const validQuantity = Math.min(newQuantity, item.availableQuantity);
                                const updatedItems = [...selectedItems];
                                const itemIndex = updatedItems.findIndex(i => i.id === item.id);
                                if (itemIndex !== -1) {
                                  updatedItems[itemIndex] = { ...updatedItems[itemIndex], quantity: validQuantity };
                                  setSelectedItems(updatedItems);
                                }
                              }} 
                              style={{ width: '100%' }}
                            />
                          ) : (
                            item.quantity
                          )}
                        </div>
                        <div style={{ flex: 1 }}>
                          <Tag color="green">{item.deviceStatus || '正常'}</Tag>
                        </div>
                        <div style={{ flex: 0.5 }}>
                          <Button 
                            danger 
                            size="small" 
                            icon={<DeleteOutlined />}
                            onClick={() => handleRemoveItem(item.id)}
                          />
                        </div>
                      </div>
                    ))}
                  </div>
                ) : (
                  <div style={{ textAlign: 'center', color: '#999', padding: 40 }}>
                    暂无已选物品
                  </div>
                )}
              </div>
            </div>
            <div style={{ flex: 1, border: '1px solid #f0f0f0', borderRadius: 4 }}>
              <div style={{ padding: 12, borderBottom: '1px solid #f0f0f0', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <div style={{ fontWeight: 'bold' }}>可选物品</div>
                <div style={{ display: 'flex', alignItems: 'center' }}>
                  <Input.Search 
                    placeholder="搜索物品" 
                    style={{ width: 150, marginRight: 8 }}
                    value={itemKeyword}
                    onChange={(e) => setItemKeyword(e.target.value)}
                    onSearch={() => fetchAvailableItems(itemKeyword, 1, selectedItemType)}
                  />
                  <span style={{ fontSize: 12, color: '#999' }}>共 {itemTotal} 条</span>
                </div>
              </div>
              <div style={{ padding: 12, borderBottom: '1px solid #f0f0f0' }}>
                <div style={{ display: 'flex', gap: 8 }}>
                  <Button 
                    type={selectedItemType === 1 ? 'primary' : 'default'} 
                    size="small"
                    style={selectedItemType === 1 ? { backgroundColor: '#1890ff', borderColor: '#1890ff' } : {}}
                    onClick={() => {
                      setSelectedItemType(1);
                      fetchAvailableItems(itemKeyword, 1, 1);
                    }}
                  >
                    专用设备
                  </Button>
                  <Button 
                    type={selectedItemType === 2 ? 'primary' : 'default'} 
                    size="small"
                    style={selectedItemType === 2 ? { backgroundColor: '#52c41a', borderColor: '#52c41a' } : {}}
                    onClick={() => {
                      setSelectedItemType(2);
                      fetchAvailableItems(itemKeyword, 1, 2);
                    }}
                  >
                    通用设备
                  </Button>
                  <Button 
                    type={selectedItemType === 3 ? 'primary' : 'default'} 
                    size="small"
                    style={selectedItemType === 3 ? { backgroundColor: '#fa8c16', borderColor: '#fa8c16' } : {}}
                    onClick={() => {
                      setSelectedItemType(3);
                      fetchAvailableItems(itemKeyword, 1, 3);
                    }}
                  >
                    耗材
                  </Button>
                  <Button 
                    type={selectedItemType === 4 ? 'primary' : 'default'} 
                    size="small"
                    style={selectedItemType === 4 ? { backgroundColor: '#722ed1', borderColor: '#722ed1' } : {}}
                    onClick={() => {
                      setSelectedItemType(4);
                      fetchAvailableItems(itemKeyword, 1, 4);
                    }}
                  >
                    原材料
                  </Button>
                </div>
              </div>
              <div style={{ padding: 12, maxHeight: 240, overflow: 'auto' }}>
                <div style={{ display: 'flex', marginBottom: 8, fontWeight: 'bold', fontSize: 12 }}>
                  <div style={{ flex: 2 }}>物品名称</div>
                  <div style={{ flex: 1 }}>品牌</div>
                  <div style={{ flex: 1 }}>型号</div>
                  <div style={{ flex: 1 }}>设备状态</div>
                  <div style={{ flex: 1 }}>可用数量</div>
                  <div style={{ flex: 0.5 }}>操作</div>
                </div>
                {availableItems.map((item, index) => (
                  <div key={item.id} style={{ display: 'flex', marginBottom: 8, alignItems: 'center', fontSize: 12 }}>
                    <div style={{ flex: 2 }}>{item.name}</div>
                    <div style={{ flex: 1 }}>{item.brand || '-'}</div>
                    <div style={{ flex: 1 }}>{item.model || '-'}</div>
                    <div style={{ flex: 1 }}>
                      <Tag color="green">{item.deviceStatus || '正常'}</Tag>
                    </div>
                    <div style={{ flex: 1 }}>{item.availableQuantity}</div>
                    <div style={{ flex: 0.5 }}>
                      <Button 
                        type="primary" 
                        size="small" 
                        icon={<SearchOutlined />}
                        onClick={() => handleSelectItem(item)}
                      />
                    </div>
                  </div>
                ))}
              </div>
              <div style={{ padding: 12, borderTop: '1px solid #f0f0f0', display: 'flex', justifyContent: 'flex-end', alignItems: 'center' }}>
                <Pagination
                  current={itemPage}
                  pageSize={10}
                  total={itemTotal}
                  onChange={(page) => {
                    setItemPage(page);
                    fetchAvailableItems(itemKeyword, page, selectedItemType);
                  }}
                  showSizeChanger={false}
                  showQuickJumper
                  showTotal={(total) => `共 ${total} 条`}
                />
              </div>
            </div>
          </div>

          <div style={{ display: 'flex', gap: 16, marginBottom: 16 }}>
            <Form.Item
              name="recipient"
              label="领用人"
              rules={[{ required: true, message: '请输入领用人' }]}
              style={{ flex: 1 }}
            >
              <Input placeholder="请输入领用人" />
            </Form.Item>
            <Form.Item
              name="handler"
              label="经办人"
              rules={[{ required: true, message: '请输入经办人' }]}
              style={{ flex: 1 }}
            >
              <Input placeholder="请输入经办人" />
            </Form.Item>
            <Form.Item
              name="warehouseKeeper"
              label="库管"
              rules={[{ required: true, message: '请输入库管' }]}
              style={{ flex: 1 }}
            >
              <Input placeholder="请输入库管" />
            </Form.Item>
            <Form.Item
              name="outboundDate"
              label="出库日期"
              rules={[{ required: true, message: '请选择出库日期' }]}
              style={{ flex: 1 }}
            >
              <DatePicker style={{ width: '100%' }} />
            </Form.Item>
          </div>

          <Form.Item
            name="remark"
            label="备注"
          >
            <div style={{ padding: 12, border: '1px solid #f0f0f0', borderRadius: 4, backgroundColor: '#fafafa', fontSize: 12, lineHeight: 1.5 }}>
              1.设备丢失：按照设备实际价格赔偿（不高于市场价）；
              2.设备损坏但仍能使用：按照设备实际价格50%赔偿；
              3.设备损坏不能使用：按照设备实际价格赔偿（不高于市场价）。
            </div>
          </Form.Item>

          <Form.Item>
            <Space style={{ float: 'right' }}>
              <Button onClick={handlePreview}>预览</Button>
              <Button onClick={() => setModalVisible(false)}>取消</Button>
              <Button type="primary" htmlType="submit">确定</Button>
            </Space>
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title="项目出库单预览"
        open={previewModalVisible}
        onCancel={() => setPreviewModalVisible(false)}
        footer={[
          <Button key="print" icon={<EyeOutlined />}>打印</Button>,
          <Button key="pdf" icon={<EyeOutlined />}>保存PDF</Button>,
          <Button key="close" type="primary" onClick={() => setPreviewModalVisible(false)}>关闭预览</Button>
        ]}
        width={800}
      >
        {previewOutbound && (
          <div className="outbound-preview">
            <h2 style={{ textAlign: 'center', marginBottom: 20 }}>项目出库单</h2>
            <div style={{ display: 'flex', gap: 16, marginBottom: 16 }}>
              <div style={{ flex: 1 }}>
                <div style={{ marginBottom: 8 }}>
                  <strong>出库单号：</strong>{previewOutbound.outboundNumber}
                </div>
                <div style={{ marginBottom: 8 }}>
                  <strong>出库类型：</strong>{previewOutbound.outboundType}
                </div>
                <div style={{ marginBottom: 8 }}>
                  <strong>物流方式：</strong>{previewOutbound.logisticsMethod === 1 ? '随身携带' : 
                    previewOutbound.logisticsMethod === 2 ? '跨越物流' : 
                    previewOutbound.logisticsMethod === 3 ? '德邦物流' : 
                    previewOutbound.logisticsMethod === 4 ? '顺丰物流' : '其他'}
                </div>
              </div>
              <div style={{ flex: 1 }}>
                <div style={{ marginBottom: 8 }}>
                  <strong>项目名称：</strong>{previewOutbound.projectName}
                </div>
                <div style={{ marginBottom: 8 }}>
                  <strong>项目负责人：</strong>{previewOutbound.projectManager}
                </div>
                <div style={{ marginBottom: 8 }}>
                  <strong>联系电话：</strong>{previewOutbound.contactPhone}
                </div>
              </div>
            </div>
            <div style={{ marginBottom: 16 }}>
              <div style={{ marginBottom: 8 }}>
                <strong>项目时间：</strong>{previewOutbound.projectTime}
              </div>
              <div style={{ marginBottom: 8 }}>
                <strong>预计归还日期：</strong>{previewOutbound.returnDate ? new Date(previewOutbound.returnDate).toLocaleDateString() : ''}
              </div>
              <div style={{ marginBottom: 8 }}>
                <strong>使用地：</strong>{previewOutbound.usageLocation || '-'}
              </div>
            </div>
            <div style={{ marginBottom: 16 }}>
              <strong>物品清单</strong>
              <div style={{ marginTop: 8, border: '1px solid #f0f0f0', borderRadius: 4, overflow: 'hidden' }}>
                <div style={{ display: 'flex', backgroundColor: '#fafafa', padding: 8, fontWeight: 'bold', fontSize: 12 }}>
                  <div style={{ flex: 0.5 }}>序号</div>
                  <div style={{ flex: 1 }}>物品类型</div>
                  <div style={{ flex: 2 }}>物品名称</div>
                  <div style={{ flex: 1 }}>设备编号</div>
                  <div style={{ flex: 1 }}>品牌</div>
                  <div style={{ flex: 1 }}>型号</div>
                  <div style={{ flex: 0.5 }}>数量</div>
                  <div style={{ flex: 1 }}>单位</div>
                  <div style={{ flex: 1 }}>设备状态</div>
                  <div style={{ flex: 1 }}>配件</div>
                  <div style={{ flex: 1 }}>备注</div>
                </div>
                {previewOutbound.items.map((item, index) => (
                  <div key={item.id} style={{ display: 'flex', padding: 8, borderTop: '1px solid #f0f0f0', fontSize: 12 }}>
                    <div style={{ flex: 0.5 }}>{index + 1}</div>
                    <div style={{ flex: 1 }}>
                      <Tag color={item.itemType === 1 ? 'blue' : item.itemType === 2 ? 'green' : item.itemType === 3 ? 'orange' : 'purple'}>
                        {item.itemTypeName}
                      </Tag>
                    </div>
                    <div style={{ flex: 2 }}>{item.name}</div>
                    <div style={{ flex: 1 }}>{item.deviceCode || '-'}</div>
                    <div style={{ flex: 1 }}>{item.brand || '-'}</div>
                    <div style={{ flex: 1 }}>{item.model || '-'}</div>
                    <div style={{ flex: 0.5 }}>{item.quantity}</div>
                    <div style={{ flex: 1 }}>{item.unit || '-'}</div>
                    <div style={{ flex: 1 }}>
                      <Tag color="green">{item.deviceStatus || '正常'}</Tag>
                    </div>
                    <div style={{ flex: 1 }}>{item.accessories || '-'}</div>
                    <div style={{ flex: 1 }}>{item.remark || '-'}</div>
                  </div>
                ))}
              </div>
            </div>
            <div style={{ marginBottom: 16 }}>
              <strong>出库图片</strong>
              <div style={{ marginTop: 8 }}>无图片</div>
            </div>
            <div style={{ display: 'flex', gap: 16, marginBottom: 16 }}>
              <div style={{ flex: 1 }}>
                <div style={{ marginBottom: 8 }}>
                  <strong>领用人：</strong>{previewOutbound.recipient}
                </div>
              </div>
              <div style={{ flex: 1 }}>
                <div style={{ marginBottom: 8 }}>
                  <strong>经办人：</strong>{previewOutbound.handler}
                </div>
              </div>
              <div style={{ flex: 1 }}>
                <div style={{ marginBottom: 8 }}>
                  <strong>库管：</strong>{previewOutbound.warehouseKeeper}
                </div>
              </div>
              <div style={{ flex: 1 }}>
                <div style={{ marginBottom: 8 }}>
                  <strong>出库日期：</strong>{previewOutbound.outboundDate ? new Date(previewOutbound.outboundDate).toLocaleDateString() : ''}
                </div>
              </div>
            </div>
            <div style={{ marginBottom: 16 }}>
              <strong>备注</strong>
              <div style={{ marginTop: 8, padding: 12, border: '1px solid #f0f0f0', borderRadius: 4, backgroundColor: '#fafafa', fontSize: 12, lineHeight: 1.5 }}>
                1.设备丢失：按照设备实际价格赔偿（不高于市场价）；
                2.设备损坏但仍能使用：按照设备实际价格50%赔偿；
                3.设备损坏不能使用：按照设备实际价格赔偿（不高于市场价）。
              </div>
            </div>
          </div>
        )}
      </Modal>

      <Modal
        title={`选择${itemDetailList[0]?.itemTypeName || currentItem?.itemTypeName}：${itemDetailList[0]?.itemName || currentItem?.name}`}
        open={itemDetailModalVisible}
        onCancel={() => setItemDetailModalVisible(false)}
        footer={[
          <Button key="close" type="primary" onClick={() => setItemDetailModalVisible(false)}>关闭</Button>
        ]}
        width={800}
      >
        <div style={{ marginBottom: 16 }}>
          <Input.Search 
            placeholder="输入设备编号后三位数字，多个用逗号分隔" 
            style={{ width: '100%' }}
            value={itemDetailKeyword}
            onChange={(e) => setItemDetailKeyword(e.target.value)}
            onSearch={() => {
              // 这里可以根据关键词过滤itemDetailList
              if (itemDetailKeyword) {
                const keywords = itemDetailKeyword.split(',').map(k => k.trim());
                const filtered = itemDetailList.filter(item => 
                  keywords.some(keyword => 
                    item.deviceCode && item.deviceCode.slice(-3) === keyword
                  )
                );
                setItemDetailList(filtered);
              } else {
                // 重置为全部
                fetchItemDetailsByType(currentItem);
              }
            }}
          />
        </div>
        <div style={{ maxHeight: 400, overflow: 'auto' }}>
        <div style={{ display: 'flex', marginBottom: 8, fontWeight: 'bold', fontSize: 12 }}>
          <div style={{ flex: 2 }}>设备名称</div>
          <div style={{ flex: 2 }}>设备编号</div>
          <div style={{ flex: 1 }}>品牌</div>
          <div style={{ flex: 1 }}>型号</div>
          <div style={{ flex: 1 }}>数量</div>
          <div style={{ flex: 1 }}>单位</div>
          <div style={{ flex: 1 }}>设备状态</div>
          <div style={{ flex: 1 }}>配件</div>
          <div style={{ flex: 1 }}>备注</div>
          <div style={{ flex: 0.5 }}>操作</div>
        </div>
        {itemDetailList.map((detail, index) => (
          <div key={detail.id} style={{ display: 'flex', marginBottom: 8, alignItems: 'center', fontSize: 12 }}>
            <div style={{ flex: 2 }}>{detail.itemName}</div>
            <div style={{ flex: 2 }}>{detail.deviceCode}</div>
            <div style={{ flex: 1 }}>{detail.brand}</div>
            <div style={{ flex: 1 }}>{detail.model}</div>
            <div style={{ flex: 1 }}>
              {detail.itemType === 3 ? (
                <Input 
                  type="number" 
                  min="1" 
                  max={detail.availableQuantity} 
                  value={detail.quantity} 
                  onChange={(e) => {
                    const newQuantity = parseInt(e.target.value) || 1;
                    const updatedDetails = [...itemDetailList];
                    updatedDetails[index] = { ...detail, quantity: newQuantity };
                    setItemDetailList(updatedDetails);
                  }} 
                  style={{ width: '80%' }}
                />
              ) : (
                detail.quantity
              )}
            </div>
            <div style={{ flex: 1 }}>{detail.unit}</div>
            <div style={{ flex: 1 }}>
              <Tag color="green">{detail.deviceStatus}</Tag>
            </div>
            <div style={{ flex: 1 }}>{detail.accessories}</div>
            <div style={{ flex: 1 }}>{detail.remark}</div>
            <div style={{ flex: 0.5 }}>
              <Button 
                type="primary" 
                size="small" 
                icon={<PlusOutlined />}
                onClick={() => handleAddItemDetail(detail)}
              />
            </div>
          </div>
        ))}
      </div>
      </Modal>
    </div>
  );
};

export default ProjectOutboundManagement