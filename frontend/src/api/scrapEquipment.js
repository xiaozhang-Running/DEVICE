import request from '../utils/request';

export const scrapEquipmentApi = {
  // 获取所有报废设备
  getScrapEquipments: (params) => request.get('/scrapequipments', { params }),
  
  // 获取单个报废设备
  getScrapEquipment: (id) => request.get(`/scrapequipments/${id}`),
  
  // 创建报废设备
  createScrapEquipment: (data) => request.post('/scrapequipments', data),
  
  // 更新报废设备
  updateScrapEquipment: (id, data) => request.put(`/scrapequipments/${id}`, data),
  
  // 删除报废设备
  deleteScrapEquipment: (id) => request.delete(`/scrapequipments/${id}`),
  
  // 搜索报废设备
  searchScrapEquipments: (keyword) => request.get(`/scrapequipments/search/${keyword}`),
  
  // 报废设备
  scrapDevice: (data) => request.post('/scrapequipments/scrap-device', data)
};
