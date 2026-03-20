import React, { useState, useEffect } from 'react'
import { Layout, Menu, Button, message } from 'antd'
import {
  HomeOutlined,
  SettingOutlined,
  DatabaseOutlined,
  HistoryOutlined,
  ShoppingOutlined,
  BarChartOutlined,
  ExportOutlined,
  ImportOutlined,
  UnorderedListOutlined,
  UserOutlined,
  ToolOutlined,
  LogoutOutlined
} from '@ant-design/icons'
import DeviceManagement from './pages/DeviceManagement'
import SpecialEquipmentManagement from './pages/SpecialEquipmentManagement'
import GeneralEquipmentManagement from './pages/GeneralEquipmentManagement'
import InventoryManagement from './pages/InventoryManagement'
import LogManagement from './pages/LogManagement'
import Dashboard from './pages/Dashboard'
import ConsumableManagement from './pages/ConsumableManagement'
import ProjectOutboundManagement from './pages/ProjectOutboundManagement'
import RawMaterialInbound from './pages/RawMaterialInbound'
import EquipmentPurchaseInbound from './pages/EquipmentPurchaseInbound'
import ProjectInbound from './pages/ProjectInbound'
import ConsumableOutbound from './pages/ConsumableOutbound'
import UsageOutbound from './pages/UsageOutbound'
import RawMaterialManagement from './pages/RawMaterialManagement'
import RepairEquipmentManagement from './pages/RepairEquipmentManagement'
import UserManagement from './pages/UserManagement'
import RawMaterialOutbound from './pages/RawMaterialOutbound'
import './App.css'

const { Header, Content, Sider } = Layout

function App() {
  const [current, setCurrent] = useState('dashboard')
  const [user, setUser] = useState(null)

  // 模拟用户登录状态
  useEffect(() => {
    const userStr = localStorage.getItem('user')
    if (userStr) {
      setUser(JSON.parse(userStr))
    } else {
      // 模拟登录
      const mockUser = {
        id: 1,
        username: 'admin',
        role: 'Admin'
      }
      localStorage.setItem('user', JSON.stringify(mockUser))
      setUser(mockUser)
    }
  }, [])

  const handleMenuClick = (e) => {
    setCurrent(e.key)
  }

  const handleLogout = () => {
    localStorage.removeItem('user')
    setUser(null)
    setCurrent('dashboard')
    message.success('退出登录成功')
  }

  const renderContent = () => {
    switch (current) {
      case 'dashboard':
        return <Dashboard />
      case 'inventory':
        return <InventoryManagement />
      case 'special':
        return <SpecialEquipmentManagement />
      case 'general':
        return <GeneralEquipmentManagement />
      case 'consumables':
        return <ConsumableManagement />
      case 'rawMaterials':
        return <RawMaterialManagement />
      case 'rawMaterialInbound':
        return <RawMaterialInbound />
      case 'equipmentPurchaseInbound':
        return <EquipmentPurchaseInbound />
      case 'projectInbound':
        return <ProjectInbound />
      case 'rawMaterialOutbound':
        return <RawMaterialOutbound />
      case 'projectOutbound':
        return <ProjectOutboundManagement />
      case 'consumableOutbound':
        return <ConsumableOutbound />
      case 'usageOutbound':
        return <UsageOutbound />
      case 'repair':
        return <RepairEquipmentManagement />
      case 'users':
        return <UserManagement />
      case 'logs':
        return <LogManagement />
      default:
        return <Dashboard />
    }
  }

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Header style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', background: '#001529' }}>
        <div className="logo" style={{ color: '#fff', fontSize: '18px', fontWeight: 'bold' }}>元动未来</div>
        {user && (
          <div style={{ display: 'flex', alignItems: 'center', gap: 16 }}>
            <span style={{ color: '#fff' }}>欢迎，{user.username}</span>
            <Button type="primary" danger icon={<LogoutOutlined />} onClick={handleLogout}>
              退出登录
            </Button>
          </div>
        )}
      </Header>
      <Layout>
        <Sider width={200} style={{ background: '#001529' }}>
          <Menu
            mode="inline"
            selectedKeys={[current]}
            style={{ height: '100%', borderRight: 0, background: '#001529' }}
            onSelect={handleMenuClick}
            theme="dark"
            items={[
              {
                key: 'dashboard',
                icon: <BarChartOutlined />,
                label: '仪表板'
              },
              {
                key: 'inventory',
                icon: <DatabaseOutlined />,
                label: '库存管理'
              },
              {
                key: 'special',
                icon: <SettingOutlined />,
                label: '专用设备管理'
              },
              {
                key: 'general',
                icon: <SettingOutlined />,
                label: '通用设备管理'
              },
              {
                key: 'consumables',
                icon: <ShoppingOutlined />,
                label: '耗材管理'
              },
              {
                key: 'rawMaterials',
                icon: <UnorderedListOutlined />,
                label: '原材料管理'
              },
              {
                key: 'inbound',
                icon: <ImportOutlined />,
                label: '入库管理',
                children: [
                  {
                    key: 'rawMaterialInbound',
                    label: '原材料入库'
                  },
                  {
                    key: 'equipmentPurchaseInbound',
                    label: '设备采购入库'
                  },
                  {
                    key: 'projectInbound',
                    label: '项目入库'
                  }
                ]
              },
              {
                key: 'outbound',
                icon: <ExportOutlined />,
                label: '出库管理',
                children: [
                  {
                    key: 'rawMaterialOutbound',
                    label: '原材料出库'
                  },
                  {
                    key: 'projectOutbound',
                    label: '项目出库'
                  }
                ]
              },
              {
                key: 'repair',
                icon: <ToolOutlined />,
                label: '待维修设备管理'
              },
              {
                key: 'users',
                icon: <UserOutlined />,
                label: '用户管理'
              },
              {
                key: 'logs',
                icon: <HistoryOutlined />,
                label: '日志管理'
              }
            ]}
          />
        </Sider>
        <Layout style={{ padding: '24px' }}>
          <Content
            style={{
              background: '#fff',
              padding: 24,
              margin: 0,
              minHeight: 280
            }}
          >
            {renderContent()}
          </Content>
        </Layout>
      </Layout>
    </Layout>
  )
}

export default App