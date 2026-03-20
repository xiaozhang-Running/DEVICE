<template>
  <div class="repair-equipment-management">
    <!-- 状态卡片 -->
    <div class="status-cards">
      <div class="status-card">
        <div class="status-title">待维修</div>
        <div class="status-count">0</div>
      </div>
      <div class="status-card">
        <div class="status-title">维修中</div>
        <div class="status-count">0</div>
      </div>
      <div class="status-card">
        <div class="status-title">已完成</div>
        <div class="status-count">0</div>
      </div>
    </div>

    <!-- 搜索和筛选 -->
    <div class="search-filter">
      <el-input
        v-model="searchKeyword"
        placeholder="搜索"
        style="width: 200px;"
        @keyup.enter="handleSearch"
      />
      <el-button type="primary" @click="handleSearch">筛选</el-button>
    </div>

    <!-- 数据表格 -->
    <el-table
      :data="equipments"
      style="width: 100%"
      empty-text="No data"
    >
      <el-table-column prop="deviceName" label="设备名称" width="120" />
      <el-table-column prop="deviceCode" label="设备编号" width="120" />
      <el-table-column prop="deviceType" label="设备类型" width="100" />
      <el-table-column prop="quantity" label="数量" width="80" />
      <el-table-column prop="status" label="状态" width="100" />
      <el-table-column prop="repairDate" label="维修时间" width="150" />
      <el-table-column prop="repairPerson" label="维修人员" width="120" />
      <el-table-column prop="remark" label="备注" />
    </el-table>
  </div>
</template>

<script>
export default {
  name: 'RepairEquipmentManagement',
  data() {
    return {
      searchKeyword: '',
      equipments: []
    }
  },
  mounted() {
    this.fetchEquipments()
  },
  methods: {
    async fetchEquipments() {
      try {
        const response = await fetch('/api/ScrapEquipments')
        const data = await response.json()
        if (data.success) {
          // 转换后端数据为前端所需格式
          this.equipments = data.data.map(item => ({
            ...item,
            status: '待维修', // 默认状态
            repairDate: item.scrapDate,
            repairPerson: item.scrappedBy
          }))
        }
      } catch (error) {
        console.error('Error fetching equipments:', error)
      }
    },
    handleSearch() {
      this.fetchEquipments()
    }
  }
}
</script>

<style scoped>
.repair-equipment-management {
  padding: 20px;
}

.status-cards {
  display: flex;
  gap: 20px;
  margin-bottom: 20px;
}

.status-card {
  flex: 1;
  padding: 20px;
  background: #f5f7fa;
  border-radius: 8px;
  text-align: center;
}

.status-title {
  font-size: 14px;
  color: #606266;
  margin-bottom: 8px;
}

.status-count {
  font-size: 24px;
  font-weight: bold;
}

.search-filter {
  margin-bottom: 20px;
  display: flex;
  gap: 10px;
}
</style>