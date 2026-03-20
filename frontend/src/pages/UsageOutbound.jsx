import React, { useState, useEffect } from 'react'
import { Table, Button, Modal, Form, Input, Select, DatePicker, message, Space, Popconfirm } from 'antd'
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons'
import dayjs from 'dayjs'

const { Option } = Select
const { TextArea } = Input

function UsageOutbound() {
  const [form] = Form.useForm()
  const [dataSource, setDataSource] = useState([])
  const [loading, setLoading] = useState(false)
  const [modalVisible, setModalVisible] = useState(false)
  const [editingRecord, setEditingRecord] = useState(null)
  const [currentPage, setCurrentPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [total, setTotal] = useState(0)

  // 部门列表（模拟数据）
  const departments = [
    { id: 1, name: '部门A' },
    { id: 2, name: '部门B' },
    { id: 3, name: '部门C' }
  ]

  // 设备列表（模拟数据）
  const equipmentTypes = [
    { id: 1, name: '专用设备' },
    { id: 2, name: '通用设备' }
  ]

  // 加载使用出库数据
  const loadUsageOutbounds = async () => {
    setLoading(true)
    try {
      // 模拟API调用
      const mockData = {
        items: [
          {
            id: 1,
            departmentId: 1,
            departmentName: '部门A',
            outboundDate: '2026-03-15',
            totalAmount: 25000,
            remark: '部门A设备使用',
            items: [
              { id: 1, equipmentName: '设备1', model: 'Model-001', quantity: 1, price: 10000 },
              { id: 2, equipmentName: '设备2', model: 'Model-002', quantity: 1, price: 15000 }
            ]
          },
          {
            id: 2,
            departmentId: 2,
            departmentName: '部门B',
            outboundDate: '2026-03-16',
            totalAmount: 10000,
            remark: '部门B设备使用',
            items: [
              { id: 3, equipmentName: '设备3', model: 'Model-003', quantity: 1, price: 10000 }
            ]
          }
        ],
        totalCount: 2
      }
      setDataSource(mockData.items)
      setTotal(mockData.totalCount)
    } catch (error) {
      message.error('加载使用出库数据失败')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadUsageOutbounds()
  }, [currentPage, pageSize])

  // 打开添加/编辑模态框
  const openModal = (record = null) => {
    setEditingRecord(record)
    if (record) {
      form.setFieldsValue({
        ...record,
        departmentId: record.departmentId,
        outboundDate: record.outboundDate ? dayjs(record.outboundDate) : null,
        items: record.items || []
      })
    } else {
      form.resetFields()
      form.setFieldsValue({
        items: []
      })
    }
    setModalVisible(true)
  }

  // 关闭模态框
  const closeModal = () => {
    setModalVisible(false)
    setEditingRecord(null)
    form.resetFields()
  }

  // 保存使用出库
  const saveUsageOutbound = async () => {
    try {
      const values = await form.validateFields()
      message.success(editingRecord ? '更新使用出库成功' : '创建使用出库成功')
      closeModal()
      loadUsageOutbounds()
    } catch (error) {
      message.error('保存失败，请检查输入')
    }
  }

  // 删除使用出库
  const deleteUsageOutbound = async (id) => {
    try {
      message.success('删除使用出库成功')
      loadUsageOutbounds()
    } catch (error) {
      message.error('删除失败')
    }
  }



  // 表格列配置
  const columns = [
    {
      title: '部门名称',
      dataIndex: 'departmentName',
      key: 'departmentName'
    },
    {
      title: '出库日期',
      dataIndex: 'outboundDate',
      key: 'outboundDate'
    },
    {
      title: '出库数量',
      dataIndex: 'items',
      key: 'items',
      render: (items) => items ? items.length : 0
    },
    {
      title: '总金额',
      dataIndex: 'totalAmount',
      key: 'totalAmount',
      render: (text) => `¥${text}`
    },
    {
      title: '备注',
      dataIndex: 'remark',
      key: 'remark'
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
            title="确定要删除这个使用出库吗？"
            onConfirm={() => deleteUsageOutbound(record.id)}
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
        <h2>使用出库</h2>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => openModal()}>
          添加使用出库
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
        title={editingRecord ? '编辑使用出库' : '添加使用出库'}
        open={modalVisible}
        onCancel={closeModal}
        onOk={saveUsageOutbound}
        width={800}
      >
        <Form form={form} layout="vertical">
          <Form.Item
            name="departmentId"
            label="部门名称"
            rules={[{ required: true, message: '请选择部门' }]}
          >
            <Select placeholder="请选择部门">
              {departments.map(department => (
                <Option key={department.id} value={department.id}>
                  {department.name}
                </Option>
              ))}
            </Select>
          </Form.Item>

          <Form.Item
            name="outboundDate"
            label="出库日期"
            rules={[{ required: true, message: '请选择出库日期' }]}
          >
            <DatePicker style={{ width: '100%' }} />
          </Form.Item>

          <Form.List name="items">
              {(fields, { add, remove }) => (
                <div>
                  {fields.map((field, index) => (
                    <div key={field.key} style={{ marginBottom: 16, padding: 16, border: '1px solid #f0f0f0', borderRadius: 4 }}>
                      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 8 }}>
                        <h4>设备 {index + 1}</h4>
                        <Button danger onClick={() => remove(index)}>删除</Button>
                      </div>
                      <Form.Item
                        {...field}
                        name={[field.name, 'equipmentName']}
                        fieldKey={[field.fieldKey, 'equipmentName']}
                        label="设备名称"
                        rules={[{ required: true, message: '请输入设备名称' }]}
                      >
                        <Input placeholder="请输入设备名称" />
                      </Form.Item>
                      <Form.Item
                        {...field}
                        name={[field.name, 'model']}
                        fieldKey={[field.fieldKey, 'model']}
                        label="型号"
                        rules={[{ required: true, message: '请输入型号' }]}
                      >
                        <Input placeholder="请输入型号" />
                      </Form.Item>
                      <Form.Item
                        {...field}
                        name={[field.name, 'quantity']}
                        fieldKey={[field.fieldKey, 'quantity']}
                        label="数量"
                        rules={[{ required: true, message: '请输入数量' }, { type: 'number', min: 1 }]}
                      >
                        <Input type="number" placeholder="请输入数量" />
                      </Form.Item>
                      <Form.Item
                        {...field}
                        name={[field.name, 'price']}
                        fieldKey={[field.fieldKey, 'price']}
                        label="单价"
                        rules={[{ required: true, message: '请输入单价' }, { type: 'number', min: 0 }]}
                      >
                        <Input type="number" placeholder="请输入单价" />
                      </Form.Item>
                    </div>
                  ))}
                  <Button type="dashed" onClick={() => add({})} style={{ width: '100%' }}>
                    <PlusOutlined /> 添加设备
                  </Button>
                </div>
              )}
            </Form.List>

          <Form.Item
            name="remark"
            label="备注"
          >
            <TextArea rows={4} placeholder="请输入备注" />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  )
}

export default UsageOutbound