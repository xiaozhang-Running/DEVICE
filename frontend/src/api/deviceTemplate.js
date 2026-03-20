import request from '../utils/request';

export const deviceTemplateApi = {
  // 获取专用设备模板
  getSpecialEquipmentTemplates: () => request.get('/devicetemplates/special-equipment'),
  
  // 获取通用设备模板
  getGeneralEquipmentTemplates: () => request.get('/devicetemplates/general-equipment'),
  
  // 获取耗材模板
  getConsumableTemplates: () => request.get('/devicetemplates/consumables')
};
