import React, { useState, useEffect } from 'react'
import { Card, Table, Button, Form, Input, Select, DatePicker, Space, message, Badge } from 'antd'
import { SearchOutlined } from '@ant-design/icons'
import userActivityLogApi from '../api/userActivityLog'
import './LogManagement.css'

const { Option } = Select
const { RangePicker } = DatePicker

const LogManagement = () => {
  const [logs, setLogs] = useState([])
  const [loading, setLoading] = useState(false)
  const [form] = Form.useForm()
  const [searchParams, setSearchParams] = useState({})
  const [pageSize, setPageSize] = useState(20)
  const [currentPage, setCurrentPage] = useState(1)
  const [totalCount, setTotalCount] = useState(0)

  const activityTypes = [
    { value: 'Login', label: '登录' },
    { value: 'Logout', label: '登出' },
    { value: 'Create', label: '创建' },
    { value: 'Update', label: '更新' },
    { value: 'Delete', label: '删除' }
  ]

  const fetchLogs = async (page = currentPage, size = pageSize, params = searchParams) => {
    setLoading(true)
    try {
      const response = await userActivityLogApi.getAllLogs({
        pageNumber: page,
        pageSize: size,
        ...params
      })
      
      // 直接使用响应对象，因为request拦截器已经返回了response.data
      const data = response || { items: [], totalCount: 0 }
      setLogs(data.items)
      setTotalCount(data.totalCount)
      setCurrentPage(page)
      setPageSize(size)
    } catch (error) {
      message.error('获取日志记录失败')
      console.error('Error fetching logs:', error)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchLogs()
  }, [])

  const handleSearch = async () => {
    const values = await form.validateFields()
    const params = {}
    
    if (values.userId) {
      params.userId = values.userId
    }
    
    if (values.activityType) {
      params.activityType = values.activityType
    }
    
    if (values.dateRange) {
      params.startDate = values.dateRange[0].format('YYYY-MM-DD')
      params.endDate = values.dateRange[1].format('YYYY-MM-DD')
    }
    
    if (values.keyword) {
      params.keyword = values.keyword
    }
    
    setSearchParams(params)
    fetchLogs(1, pageSize, params)
  }

  const handleReset = () => {
    form.resetFields()
    setSearchParams({})
    fetchLogs(1, pageSize, {})
  }

  const columns = [
    {
      title: '序号',
      dataIndex: 'sortOrder',
      key: 'sortOrder',
      width: 60,
      render: (text, record, index) => (currentPage - 1) * pageSize + index + 1
    },
    {
      title: '用户ID',
      dataIndex: 'userId',
      key: 'userId',
      width: 80
    },
    {
      title: '操作类型',
      dataIndex: 'activityType',
      key: 'activityType',
      width: 100,
      render: (text) => {
        const type = activityTypes.find(t => t.value === text)
        return type ? (
          <Badge 
            status={text === 'Login' ? 'success' : text === 'Delete' ? 'error' : 'processing'}
            text={type.label}
          />
        ) : text
      }
    },
    {
      title: '操作描述',
      dataIndex: 'activityDescription',
      key: 'activityDescription',
      ellipsis: true
    },
    {
      title: 'IP地址',
      dataIndex: 'ipAddress',
      key: 'ipAddress',
      width: 150
    },
    {
      title: '用户代理',
      dataIndex: 'userAgent',
      key: 'userAgent',
      width: 200,
      ellipsis: true
    },
    {
      title: '操作时间',
      dataIndex: 'createdAt',
      key: 'createdAt',
      width: 180,
      render: (text) => {
        const date = new Date(text)
        return date.toLocaleString('zh-CN')
      }
    }
  ]

  return (
    <div className="log-management">
      <Card
        title="日志管理"
        extra={
          <Space>
            <Button type="primary" icon={<SearchOutlined />} onClick={handleSearch}>
              搜索
            </Button>
            <Button onClick={handleReset}>
              重置
            </Button>
          </Space>
        }
      >
        <Form form={form} layout="inline" className="search-form">
          <Form.Item name="userId" label="用户ID">
            <Input placeholder="请输入用户ID" style={{ width: 120 }} />
          </Form.Item>
          <Form.Item name="activityType" label="操作类型">
            <Select placeholder="请选择操作类型" style={{ width: 100 }}>
              {activityTypes.map(type => (
                <Option key={type.value} value={type.value}>{type.label}</Option>
              ))}
            </Select>
          </Form.Item>
          <Form.Item name="dateRange" label="操作时间">
            <RangePicker style={{ width: 200 }} />
          </Form.Item>
          <Form.Item name="keyword" label="关键词">
            <Input placeholder="请输入关键词" style={{ width: 200 }} />
          </Form.Item>
        </Form>

        <Table
          columns={columns}
          dataSource={logs}
          rowKey={(record) => record.Id || record.id || Math.random().toString(36)}
          loading={loading}
          size="small"
          pagination={{
            current: currentPage,
            pageSize: pageSize,
            total: totalCount,
            showSizeChanger: true,
            pageSizeOptions: ['20', '50', '100', '200'],
            showTotal: (total) => `共 ${total} 条日志`,
            onChange: (page, newPageSize) => {
              if (newPageSize !== pageSize) {
                setPageSize(newPageSize)
                fetchLogs(1, newPageSize, searchParams)
              } else {
                fetchLogs(page, pageSize, searchParams)
              }
            }
          }}
          scroll={{ x: 1000 }}
        />
      </Card>
    </div>
  )
}

export default LogManagement