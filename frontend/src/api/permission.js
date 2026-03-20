import request from '../utils/request';

export const permissionApi = {
  // 获取所有权限
  getPermissions: () => request.get('/permissions'),
  
  // 获取单个权限
  getPermission: (id) => request.get(`/permissions/${id}`),
  
  // 创建权限
  createPermission: (data) => request.post('/permissions', data),
  
  // 更新权限
  updatePermission: (id, data) => request.put(`/permissions/${id}`, data),
  
  // 删除权限
  deletePermission: (id) => request.delete(`/permissions/${id}`)
};
