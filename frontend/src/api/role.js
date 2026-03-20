import request from '../utils/request';

export const roleApi = {
  // 获取所有角色
  getRoles: () => request.get('/roles'),
  
  // 获取单个角色
  getRole: (id) => request.get(`/roles/${id}`),
  
  // 创建角色
  createRole: (data) => request.post('/roles', data),
  
  // 更新角色
  updateRole: (id, data) => request.put(`/roles/${id}`, data),
  
  // 删除角色
  deleteRole: (id) => request.delete(`/roles/${id}`)
};
