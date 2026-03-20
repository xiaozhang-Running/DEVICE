import React, { useState, useEffect } from 'react'
import { Table, Button, Modal, Form, Input, Select, message, Space, Popconfirm, Tag } from 'antd'
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons'
import { userApi } from '../api/user'
import { roleApi } from '../api/role'

const { Option } = Select

function UserManagement() {
  const [form] = Form.useForm()
  const [dataSource, setDataSource] = useState([])
  const [loading, setLoading] = useState(false)
  const [modalVisible, setModalVisible] = useState(false)
  const [editingRecord, setEditingRecord] = useState(null)
  const [currentPage, setCurrentPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [total, setTotal] = useState(0)
  const [roles, setRoles] = useState([])

  // 加载角色数据
  const loadRoles = async () => {
    try {
      const response = await roleApi.getRoles()
      if (response.success && response.data) {
        setRoles(response.data)
      }
    } catch (error) {
      console.error('加载角色失败:', error)
    }
  }

  // 加载用户数据
  const loadUsers = async () => {
    setLoading(true)
    try {
      const response = await userApi.getUsers()
      if (response.success && response.data) {
        setDataSource(response.data)
        setTotal(response.data.length)
      }
    } catch (error) {
      message.error('加载用户数据失败')
      console.error('加载用户失败:', error)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadRoles()
    loadUsers()
  }, [])

  // 打开添加/编辑模态框
  const openModal = (record = null) => {
    setEditingRecord(record)
    if (record) {
      form.setFieldsValue({
        username: record.Username,
        password: '',
        name: record.Name,
        email: record.Email,
        phone: record.Phone,
        role: record.Role,
        status: record.Status
      })
    } else {
      form.resetFields()
    }
    setModalVisible(true)
  }

  // 关闭模态框
  const closeModal = () => {
    setModalVisible(false)
    setEditingRecord(null)
    form.resetFields()
  }

  // 保存用户
  const saveUser = async () => {
    try {
      const values = await form.validateFields()
      
      const userData = {
        Username: values.username,
        Name: values.name,
        Email: values.email,
        Phone: values.phone,
        Role: values.role,
        Status: values.status
      }
      
      if (values.password) {
        userData.Password = values.password
      }
      
      if (editingRecord) {
        // 更新用户
        await userApi.updateUser(editingRecord.Id, userData)
        message.success('更新用户成功')
      } else {
        // 创建用户
        await userApi.createUser(userData)
        message.success('创建用户成功')
      }
      
      closeModal()
      loadUsers()
    } catch (error) {
      message.error('保存失败，请检查输入')
      console.error('保存用户失败:', error)
    }
  }

  // 删除用户
  const deleteUser = async (id) => {
    try {
      await userApi.deleteUser(id)
      message.success('删除用户成功')
      loadUsers()
    } catch (error) {
      message.error('删除失败')
      console.error('删除用户失败:', error)
    }
  }

  // 表格列配置
  const columns = [
    {
      title: '用户名',
      dataIndex: 'Username',
      key: 'Username'
    },
    {
      title: '邮箱',
      dataIndex: 'Email',
      key: 'Email'
    },
    {
      title: '姓名',
      dataIndex: 'Name',
      key: 'Name'
    },
    {
      title: '角色',
      dataIndex: 'Role',
      key: 'Role'
    },
    {
      title: '状态',
      dataIndex: 'Status',
      key: 'Status',
      render: (status) => (
        <Tag color={status ? 'green' : 'red'}>
          {status ? '活跃' : '禁用'}
        </Tag>
      )
    },
    {
      title: '创建时间',
      dataIndex: 'CreatedAt',
      key: 'CreatedAt',
      render: (createdAt) => {
        if (!createdAt) return ''
        const date = new Date(createdAt)
        return date.toLocaleString()
      }
    },
    {
      title: '操作',
      key: 'action',
      render: (_, record) => (
        <Space size="middle">
          <Button
            type="primary"
            icon={<EditOutlined />}
            onClick={() => openModal(record)}
          >
            编辑
          </Button>
          <Popconfirm
            title="确定要删除这个用户吗？"
            onConfirm={() => deleteUser(record.Id)}
            okText="确定"
            cancelText="取消"
          >
            <Button danger icon={<DeleteOutlined />}>
              删除
            </Button>
          </Popconfirm>
        </Space>
      )
    }
  ]

  return (
    <div>
      <div style={{ marginBottom: 16, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h2>用户管理</h2>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => openModal()}>
          添加用户
        </Button>
      </div>

      <Table
        columns={columns}
        dataSource={dataSource}
        rowKey={(record) => record.Id || record.id || Math.random().toString(36)}
        loading={loading}
        pagination={{
          current: currentPage,
          pageSize: pageSize,
          total: total,
          onChange: (page, size) => {
            setCurrentPage(page)
            setPageSize(size)
          }
        }}
      />

      <Modal
        title={editingRecord ? '编辑用户' : '添加用户'}
        open={modalVisible}
        onCancel={closeModal}
        onOk={saveUser}
        width={600}
      >
        <Form form={form} layout="vertical">
          <Form.Item
            name="username"
            label="用户名"
            rules={[{ required: true, message: '请输入用户名' }]}
          >
            <Input placeholder="请输入用户名" />
          </Form.Item>

          <Form.Item
            name="password"
            label="密码"
            rules={[(!editingRecord) && { required: true, message: '请输入密码' }]}
          >
            <Input.Password placeholder="请输入密码" />
          </Form.Item>

          <Form.Item
            name="email"
            label="邮箱"
            rules={[{ required: true, message: '请输入邮箱' }, { type: 'email', message: '请输入正确的邮箱地址' }]}
          >
            <Input placeholder="请输入邮箱" />
          </Form.Item>

          <Form.Item
            name="name"
            label="姓名"
            rules={[{ required: true, message: '请输入姓名' }]}
          >
            <Input placeholder="请输入姓名" />
          </Form.Item>

          <Form.Item
            name="phone"
            label="电话"
            rules={[{ required: true, message: '请输入电话' }]}
          >
            <Input placeholder="请输入电话" />
          </Form.Item>

          <Form.Item
            name="role"
            label="角色"
            rules={[{ required: true, message: '请选择角色' }]}
          >
            <Select placeholder="请选择角色">
              {roles.map(role => (
                <Option key={role.Id} value={role.Name}>
                  {role.Name}
                </Option>
              ))}
            </Select>
          </Form.Item>

          <Form.Item
            name="status"
            label="状态"
            rules={[{ required: true, message: '请选择状态' }]}
          >
            <Select placeholder="请选择状态">
              <Option value={true}>活跃</Option>
              <Option value={false}>禁用</Option>
            </Select>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  )
}

export default UserManagement