import request from '../utils/request'

const userActivityLogApi = {
  // 获取所有日志
  getAllLogs: (params) => {
    return request({
      url: '/useractivitylogs',
      method: 'get',
      params
    })
  },

  // 按用户ID获取日志
  getLogsByUserId: (userId, params) => {
    return request({
      url: `/useractivitylogs/user/${userId}`,
      method: 'get',
      params
    })
  },

  // 按日期范围获取日志
  getLogsByDateRange: (startDate, endDate, params) => {
    return request({
      url: '/useractivitylogs/date-range',
      method: 'get',
      params: {
        startDate,
        endDate,
        ...params
      }
    })
  },

  // 按活动类型获取日志
  getLogsByActivityType: (activityType, params) => {
    return request({
      url: `/useractivitylogs/activity-type/${activityType}`,
      method: 'get',
      params
    })
  }
}

export default userActivityLogApi