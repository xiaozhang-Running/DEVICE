import request from '../utils/request'

const projectOutboundApi = {
  // 获取项目出库列表
  getProjectOutbounds: (params) => {
    return request({
      url: '/projectoutbounds',
      method: 'get',
      params
    })
  },

  // 获取单个项目出库详情
  getProjectOutbound: (id) => {
    return request({
      url: `/projectoutbounds/${id}`,
      method: 'get'
    })
  },

  // 创建项目出库
  createProjectOutbound: (data) => {
    return request({
      url: '/projectoutbounds',
      method: 'post',
      data
    })
  },

  // 更新项目出库
  updateProjectOutbound: (id, data) => {
    return request({
      url: `/projectoutbounds/${id}`,
      method: 'put',
      data
    })
  },

  // 删除项目出库
  deleteProjectOutbound: (id) => {
    return request({
      url: `/projectoutbounds/${id}`,
      method: 'delete'
    })
  },

  // 完成项目出库
  completeProjectOutbound: (id) => {
    return request({
      url: `/projectoutbounds/${id}/complete`,
      method: 'post'
    })
  },

  // 检查项目出库是否存在
  checkProjectOutboundExists: (number) => {
    return request({
      url: `/projectoutbounds/exists/${number}`,
      method: 'get'
    })
  },

  // 获取可用物品
  getAvailableItems: (keyword) => {
    return request({
      url: '/projectoutbounds/available-items',
      method: 'get',
      params: { keyword }
    })
  },

  // 分页获取可用物品
  getAvailableItemsPaged: (data) => {
    return request({
      url: '/projectoutbounds/available-items/paged',
      method: 'post',
      data
    })
  },

  // 搜索项目出库
  searchProjectOutbounds: (keyword) => {
    return request({
      url: '/projectoutbounds/search',
      method: 'get',
      params: { keyword }
    })
  }
}

export default projectOutboundApi