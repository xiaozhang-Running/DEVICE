import request from '../utils/request'

const dashboardApi = {
  // 获取仪表板概览
  getOverview: () => {
    return request({
      url: '/dashboard/overview',
      method: 'get'
    })
  },

  // 获取库存状态
  getInventoryStatus: () => {
    return request({
      url: '/dashboard/inventory-status',
      method: 'get'
    })
  },

  // 获取设备状态
  getEquipmentStatus: () => {
    return request({
      url: '/dashboard/equipment-status',
      method: 'get'
    })
  }
}

export default dashboardApi