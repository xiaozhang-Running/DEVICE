import request from '../utils/request';

export const userApi = {
  // 登录
  login: (data) => request.post('/users/login', data),
  
  // 获取所有用户
  getUsers: () => request.get('/users'),
  
  // 获取单个用户
  getUser: (id) => request.get(`/users/${id}`),
  
  // 创建用户
  createUser: (data) => request.post('/users', data),
  
  // 更新用户
  updateUser: (id, data) => request.put(`/users/${id}`, data),
  
  // 删除用户
  deleteUser: (id) => request.delete(`/users/${id}`),
  
  // 修改密码
  changePassword: (id, data) => request.post(`/users/${id}/change-password`, data),
  
  // 按角色获取用户
  getUsersByRole: (role) => request.get(`/users/role/${role}`),
  
  // 重置密码
  resetPassword: (data) => request.post('/users/reset-password', data),
  
  // 使用令牌重置密码
  resetPasswordWithToken: (data) => request.post('/users/reset-password-with-token', data),
  
  // 批量创建用户
  bulkCreateUsers: (data) => request.post('/users/bulk-create', data),
  
  // 批量删除用户
  bulkDeleteUsers: (data) => request.post('/users/bulk-delete', data),
  
  // 批量更新用户状态
  bulkUpdateUserStatus: (data) => request.post('/users/bulk-update-status', data),
  
  // 锁定用户
  lockUser: (id) => request.post(`/users/${id}/lock`),
  
  // 解锁用户
  unlockUser: (id) => request.post(`/users/${id}/unlock`)
};
