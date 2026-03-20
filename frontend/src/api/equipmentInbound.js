import request from '../utils/request'

const equipmentInboundApi = {
  // 获取设备入库列表
  getEquipmentInbounds: () => {
    return request({
      url: '/EquipmentInbounds',
      method: 'get'
    })
  },

  // 获取单个设备入库详情
  getEquipmentInbound: (id) => {
    return request({
      url: `/EquipmentInbounds/${id}`,
      method: 'get'
    })
  },

  // 创建设备入库
  createEquipmentInbound: (data) => {
    return request({
      url: '/EquipmentInbounds',
      method: 'post',
      data
    })
  },

  // 更新设备入库
  updateEquipmentInbound: (id, data) => {
    return request({
      url: `/EquipmentInbounds/${id}`,
      method: 'put',
      data
    })
  },

  // 完成设备入库
  completeEquipmentInbound: (id) => {
    return request({
      url: `/EquipmentInbounds/${id}/complete`,
      method: 'put'
    })
  },

  // 删除设备入库
  deleteEquipmentInbound: (id) => {
    return request({
      url: `/EquipmentInbounds/${id}`,
      method: 'delete'
    })
  },

  // 检查入库单编号是否存在
  checkEquipmentInboundExists: (number) => {
    return request({
      url: `/EquipmentInbounds/exists/${number}`,
      method: 'get'
    })
  }
}

export default equipmentInboundApi