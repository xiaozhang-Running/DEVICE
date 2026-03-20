import request from '../utils/request';

export const outboundOrderApi = {
  getOutboundOrders: () => request.get('/outboundorders'),
  getOutboundOrder: (id) => request.get(`/outboundorders/${id}`),
  createOutboundOrder: (data) => request.post('/outboundorders', data),
  updateOutboundOrder: (id, data) => request.put(`/outboundorders/${id}`, data),
  deleteOutboundOrder: (id) => request.delete(`/outboundorders/${id}`),
  approveOutboundOrder: (id) => request.post(`/outboundorders/${id}/approve`),
  getOutboundOrderStats: () => request.get('/outboundorders/stats')
};
