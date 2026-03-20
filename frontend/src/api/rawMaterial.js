import request from '../utils/request'

const rawMaterialApi = {
  // 获取原料列表
  getRawMaterials: () => {
    return request({
      url: '/rawmaterials',
      method: 'get'
    })
  },

  // 获取单个原料详情
  getRawMaterial: (id) => {
    return request({
      url: `/rawmaterials/${id}`,
      method: 'get'
    })
  },

  // 创建原料
  createRawMaterial: (data) => {
    return request({
      url: '/rawmaterials',
      method: 'post',
      data
    })
  },

  // 批量导入原料
  importBatch: (data) => {
    return request({
      url: '/rawmaterials/batch',
      method: 'post',
      data
    })
  },

  // 更新原料
  updateRawMaterial: (id, data) => {
    return request({
      url: `/rawmaterials/${id}`,
      method: 'put',
      data
    })
  },

  // 删除原料
  deleteRawMaterial: (id) => {
    return request({
      url: `/rawmaterials/${id}`,
      method: 'delete'
    })
  },

  // 清空所有原料
  deleteAll: () => {
    return request({
      url: '/rawmaterials/all',
      method: 'delete'
    })
  },

  // 搜索原料
  search: (keyword) => {
    return request({
      url: `/rawmaterials/search/${keyword}`,
      method: 'get'
    })
  }
}

export default rawMaterialApi