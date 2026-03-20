import request from '../utils/request'

const projectInboundApi = {
  // 获取项目入库列表
  getProjectInbounds: () => {
    return request({
      url: '/projectinbounds',
      method: 'get'
    })
  },

  // 获取单个项目入库详情
  getProjectInbound: (id) => {
    return request({
      url: `/projectinbounds/${id}`,
      method: 'get'
    })
  },

  // 创建项目入库
  createProjectInbound: (data) => {
    return request({
      url: '/projectinbounds',
      method: 'post',
      data
    })
  },

  // 更新项目入库
  updateProjectInbound: (id, data) => {
    return request({
      url: `/projectinbounds/${id}`,
      method: 'put',
      data
    })
  },

  // 删除项目入库
  deleteProjectInbound: (id) => {
    return request({
      url: `/projectinbounds/${id}`,
      method: 'delete'
    })
  },

  // 完成项目入库
  completeProjectInbound: (id) => {
    return request({
      url: `/projectinbounds/${id}/complete`,
      method: 'post'
    })
  },

  // 部分入库
  partialInbound: (id) => {
    return request({
      url: `/projectinbounds/${id}/partial-inbound`,
      method: 'post'
    })
  },

  // 检查入库单编号是否存在
  checkInboundExists: (number) => {
    return request({
      url: `/projectinbounds/exists/${number}`,
      method: 'get'
    })
  },

  // 获取可用的项目出库单
  getAvailableProjectOutbounds: () => {
    return request({
      url: '/projectinbounds/available-outbounds',
      method: 'get'
    })
  },

  // 根据ID获取项目出库单
  getProjectOutboundById: (id) => {
    return request({
      url: `/projectinbounds/outbound/${id}`,
      method: 'get'
    })
  },

  // 获取未入库的项目出库项
  getUninboundItemsByOutboundId: (outboundId) => {
    return request({
      url: `/projectinbounds/uninbound-items/${outboundId}`,
      method: 'get'
    })
  }
}

export default projectInboundApi