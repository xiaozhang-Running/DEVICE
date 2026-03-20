import request from '../utils/request'

const specialEquipmentApi = {
  // 获取所有专用设备
  getAll: (params) => {
    return request({
      url: '/SpecialEquipments',
      method: 'get',
      params
    })
  },

  // 按ID获取专用设备
  getById: (id) => {
    return request({
      url: `/SpecialEquipments/${id}`,
      method: 'get'
    })
  },

  // 创建专用设备
  create: (data) => {
    return request({
      url: '/SpecialEquipments',
      method: 'post',
      data
    })
  },

  // 更新专用设备
  update: (id, data) => {
    return request({
      url: `/SpecialEquipments/${id}`,
      method: 'put',
      data
    })
  },

  // 删除专用设备
  delete: (id) => {
    return request({
      url: `/SpecialEquipments/${id}`,
      method: 'delete'
    })
  },

  // 清空所有专用设备
  deleteAll: () => {
    return request({
      url: '/SpecialEquipments/all',
      method: 'delete'
    })
  },

  // 批量导入专用设备
  importBatch: (data) => {
    return request({
      url: '/SpecialEquipments/batch',
      method: 'post',
      data
    })
  },

  // 搜索专用设备
  search: (keyword) => {
    return request({
      url: `/SpecialEquipments/search/${keyword || 'all'}`,
      method: 'get'
    })
  }
}

export default specialEquipmentApi