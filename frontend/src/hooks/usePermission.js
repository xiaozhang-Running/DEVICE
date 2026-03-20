import { useMemo } from 'react'

const usePermission = () => {
  // 从localStorage中获取用户信息
  const user = useMemo(() => {
    const userStr = localStorage.getItem('user')
    return userStr ? JSON.parse(userStr) : null
  }, [])

  // 检查用户是否有特定权限
  const canAction = (action) => {
    // 这里可以根据实际的权限系统进行调整
    // 暂时简单实现，管理员角色拥有所有权限
    if (user?.role === 'Admin' || user?.Role === 'Admin') {
      return true
    }
    // 其他角色的权限逻辑可以在这里添加
    return false
  }

  return {
    user,
    canAction
  }
}

export default usePermission