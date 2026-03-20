import request from '../utils/request'

const rawMaterialOutboundApi = {
  // 获取原料出库列表
  getRawMaterialOutbounds: () => {
    return request({
      url: '/rawmaterialoutbounds',
      method: 'get'
    })
  },

  // 获取单个原料出库详情
  getRawMaterialOutbound: (id) => {
    return request({
      url: `/rawmaterialoutbounds/${id}`,
      method: 'get'
    })
  },

  // 创建原料出库
  createRawMaterialOutbound: (data) => {
    return request({
      url: '/rawmaterialoutbounds',
      method: 'post',
      data
    })
  },

  // 更新原料出库
  updateRawMaterialOutbound: (id, data) => {
    return request({
      url: `/rawmaterialoutbounds/${id}`,
      method: 'put',
      data
    })
  },

  // 删除原料出库
  deleteRawMaterialOutbound: (id) => {
    return request({
      url: `/rawmaterialoutbounds/${id}`,
      method: 'delete'
    })
  },

  // 检查出库单编号是否存在
  checkRawMaterialOutboundExists: (number) => {
    return request({
      url: `/rawmaterialoutbounds/exists/${number}`,
      method: 'get'
    })
  },

  // 生成出库单号
  generateOutboundNumber: () => {
    return request({
      url: '/rawmaterialoutbounds/generate-number',
      method: 'get'
    })
  },

  // 完成原料出库
  completeRawMaterialOutbound: (id) => {
    return request({
      url: `/rawmaterialoutbounds/${id}/complete`,
      method: 'put'
    })
  }
}

export default rawMaterialOutboundApi