import request from '../utils/request'

const consumableApi = {
  // 获取耗材列表
  getConsumables: (params) => {
    return request({
      url: '/consumables',
      method: 'get',
      params
    })
  },

  // 获取单个耗材详情
  getConsumable: (id) => {
    return request({
      url: `/consumables/${id}`,
      method: 'get'
    })
  },

  // 创建耗材
  createConsumable: (data) => {
    return request({
      url: '/consumables',
      method: 'post',
      data
    })
  },

  // 更新耗材
  updateConsumable: (id, data) => {
    return request({
      url: `/consumables/${id}`,
      method: 'put',
      data
    })
  },

  // 删除耗材
  deleteConsumable: (id) => {
    return request({
      url: `/consumables/${id}`,
      method: 'delete'
    })
  },

  // 搜索耗材
  searchConsumables: (keyword) => {
    return request({
      url: `/consumables/search/${keyword || 'all'}`,
      method: 'get'
    })
  },

  // 清空所有耗材
  deleteAllConsumables: () => {
    return request({
      url: '/consumables/all',
      method: 'delete'
    })
  },

  // 批量导入耗材
  importBatchConsumables: (data) => {
    return request({
      url: '/consumables/batch',
      method: 'post',
      data
    })
  }
}

export default consumableApi