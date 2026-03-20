import request from '../utils/request'

const generalEquipmentApi = {
  // 获取所有通用设备
  getGeneralEquipments: (params) => {
    return request({
      url: '/GeneralEquipments',
      method: 'get',
      params
    })
  },

  // 获取所有通用设备（别名）
  getAll: (params) => {
    return request({
      url: '/GeneralEquipments',
      method: 'get',
      params
    })
  },

  // 按ID获取通用设备
  getById: (id) => {
    return request({
      url: `/GeneralEquipments/${id}`,
      method: 'get'
    })
  },

  // 创建通用设备
  createGeneralEquipment: (data) => {
    return request({
      url: '/GeneralEquipments',
      method: 'post',
      data
    })
  },

  // 创建通用设备（别名）
  create: (data) => {
    return request({
      url: '/GeneralEquipments',
      method: 'post',
      data
    })
  },

  // 更新通用设备
  updateGeneralEquipment: (id, data) => {
    return request({
      url: `/GeneralEquipments/${id}`,
      method: 'put',
      data
    })
  },

  // 更新通用设备（别名）
  update: (id, data) => {
    return request({
      url: `/GeneralEquipments/${id}`,
      method: 'put',
      data
    })
  },

  // 删除通用设备
  deleteGeneralEquipment: (id) => {
    return request({
      url: `/GeneralEquipments/${id}`,
      method: 'delete'
    })
  },

  // 删除通用设备（别名）
  delete: (id) => {
    return request({
      url: `/GeneralEquipments/${id}`,
      method: 'delete'
    })
  },

  // 搜索通用设备
  search: (keyword) => {
    return request({
      url: `/GeneralEquipments/search/${keyword || 'all'}`,
      method: 'get'
    })
  }
}

export default generalEquipmentApi