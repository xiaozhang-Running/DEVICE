import request from '../utils/request'

const inventoryApi = {
  // 获取库存数据
  getInventory: (params) => {
    return request({
      url: '/inventory',
      method: 'get',
      params
    })
  },

  // 按ID获取库存
  getById: (id) => {
    return request({
      url: `/inventory/${id}`,
      method: 'get'
    })
  },

  // 按专用设备ID获取库存
  getBySpecialEquipmentId: (specialEquipmentId) => {
    return request({
      url: `/inventory/special/${specialEquipmentId}`,
      method: 'get'
    })
  },

  // 按通用设备ID获取库存
  getByGeneralEquipmentId: (generalEquipmentId) => {
    return request({
      url: `/inventory/general/${generalEquipmentId}`,
      method: 'get'
    })
  },

  // 创建库存
  create: (data) => {
    return request({
      url: '/inventory',
      method: 'post',
      data
    })
  },

  // 更新库存
  update: (id, data) => {
    return request({
      url: `/inventory/${id}`,
      method: 'put',
      data
    })
  },

  // 删除库存
  delete: (id) => {
    return request({
      url: `/inventory/${id}`,
      method: 'delete'
    })
  },

  // 获取低库存预警
  getLowStock: (threshold) => {
    return request({
      url: `/inventory/low-stock`,
      method: 'get',
      params: { threshold }
    })
  },

  // 获取零库存预警
  getZeroStock: () => {
    return request({
      url: `/inventory/zero-stock`,
      method: 'get'
    })
  },

  // 增加库存
  addStock: (inventoryId, quantity, reason, reference) => {
    return request({
      url: `/inventory/add-stock`,
      method: 'post',
      data: {
        inventoryId,
        quantity,
        reason,
        reference
      }
    })
  },

  // 减少库存
  removeStock: (inventoryId, quantity, reason, reference) => {
    return request({
      url: `/inventory/remove-stock`,
      method: 'post',
      data: {
        inventoryId,
        quantity,
        reason,
        reference
      }
    })
  },

  // 创建库存交易
  createTransaction: (data) => {
    return request({
      url: `/inventory/transactions`,
      method: 'post',
      data
    })
  },

  // 获取库存交易记录
  getTransactions: (inventoryId) => {
    return request({
      url: `/inventory/transactions/${inventoryId}`,
      method: 'get'
    })
  },

  // 按日期范围获取库存交易记录
  getTransactionsByDateRange: (startDate, endDate) => {
    return request({
      url: `/inventory/transactions/date-range`,
      method: 'get',
      params: {
        startDate,
        endDate
      }
    })
  },

  // 获取库存报表
  getInventoryReport: (startDate, endDate) => {
    return request({
      url: `/inventory/report`,
      method: 'get',
      params: {
        startDate,
        endDate
      }
    })
  },

  // 获取分类库存报表
  getCategoryReport: () => {
    return request({
      url: `/inventory/report/category`,
      method: 'get'
    })
  }
}

export default inventoryApi