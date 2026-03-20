import request from '../utils/request';

export const dataApi = {
  // 清除所有数据
  clearAllData: () => request.delete('/data/clear-all')
};
