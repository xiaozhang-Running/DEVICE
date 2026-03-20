import request from '../utils/request'

const rawMaterialInboundApi = {
  // 获取原料入库列表
  getRawMaterialInbounds: () => {
    return request({
      url: '/rawmaterialinbounds',
      method: 'get'
    })
  },

  // 获取单个原料入库详情
  getRawMaterialInbound: (id) => {
    return request({
      url: `/rawmaterialinbounds/${id}`,
      method: 'get'
    })
  },

  // 创建原料入库
  createRawMaterialInbound: (data) => {
    return request({
      url: '/rawmaterialinbounds',
      method: 'post',
      data
    })
  },

  // 更新原料入库
  updateRawMaterialInbound: (id, data) => {
    return request({
      url: `/rawmaterialinbounds/${id}`,
      method: 'put',
      data
    })
  },

  // 完成原料入库
  completeRawMaterialInbound: (id) => {
    return request({
      url: `/rawmaterialinbounds/${id}/complete`,
      method: 'put'
    })
  },

  // 删除原料入库
  deleteRawMaterialInbound: (id) => {
    return request({
      url: `/rawmaterialinbounds/${id}`,
      method: 'delete'
    })
  },

  // 检查入库单编号是否存在
  checkRawMaterialInboundExists: (number) => {
    return request({
      url: `/rawmaterialinbounds/exists/${number}`,
      method: 'get'
    })
  }
}

export default rawMaterialInboundApi