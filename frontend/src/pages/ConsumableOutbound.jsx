import React, { useState, useEffect } from 'react'
import { Table, Button, Modal, Form, Input, Select, DatePicker, message, Space, Popconfirm } from 'antd'
import { PlusOutlined, EditOutlined, DeleteOutlined, SearchOutlined, ReloadOutlined } from '@ant-design/icons'
import dayjs from 'dayjs'

const { Option } = Select
const { TextArea } = Input

function ConsumableOutbound() {
  const [form] = Form.useForm()
  const [dataSource, setDataSource] = useState([])
  const [loading, setLoading] = useState(false)
  const [modalVisible, setModalVisible] = useState(false)
  const [viewModalVisible, setViewModalVisible] = useState(false)
  const [editingRecord, setEditingRecord] = useState(null)
  const [viewingRecord, setViewingRecord] = useState(null)
  const [currentPage, setCurrentPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [total, setTotal] = useState(0)

  // 耗材列表（模拟数据）
  const consumables = [
    { id: 1, name: '耗材1', specification: '规格1', availableQuantity: 200, unit: '个', price: 20 },
    { id: 2, name: '耗材2', specification: '规格2', availableQuantity: 150, unit: '个', price: 60 },
    { id: 3, name: '耗材3', specification: '规格3', availableQuantity: 100, unit: '个', price: 100 }
  ]

  // 加载耗材出库数据
  const loadConsumableOutbounds = async () => {
    setLoading(true)
    try {
      // 模拟API调用
      const mockData = {
        items: [
          {
            id: 1,
            outboundOrderNumber: 'OUT-2023-001',
            outboundDate: '2023-03-15',
            outboundDescription: '项目A耗材出库',
            totalQuantity: 15,
            totalAmount: 500,
            receiver: '张三',
            operator: '李四',
            remark: '正常出库',
            items: [
              { id: 1, consumableId: 1, consumableName: '耗材1', specification: '规格1', quantity: 10, unit: '个', price: 20, availableQuantity: 190, remark: '' },
              { id: 2, consumableId: 2, consumableName: '耗材2', specification: '规格2', quantity: 5, unit: '个', price: 60, availableQuantity: 145, remark: '' }
            ]
          },
          {
            id: 2,
            outboundOrderNumber: 'OUT-2023-002',
            outboundDate: '2023-03-16',
            outboundDescription: '项目B耗材出库',
            totalQuantity: 3,
            totalAmount: 300,
            receiver: '王五',
            operator: '赵六',
            remark: '紧急出库',
            items: [
              { id: 3, consumableId: 3, consumableName: '耗材3', specification: '规格3', quantity: 3, unit: '个', price: 100, availableQuantity: 97, remark: '' }
            ]
          }
        ],
        totalCount: 2
      }
      setDataSource(mockData.items)
      setTotal(mockData.totalCount)
    } catch (error) {
      message.error('加载耗材出库数据失败')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadConsumableOutbounds()
  }, [currentPage, pageSize])

  // 生成出库单号
  const generateOutboundOrderNumber = () => {
    const date = new Date()
    const year = date.getFullYear()
    const month = String(date.getMonth() + 1).padStart(2, '0')
    const day = String(date.getDate()).padStart(2, '0')
    const random = Math.floor(Math.random() * 10000).toString().padStart(4, '0')
    return `OUT-${year}${month}${day}${random}`
  }

  // 打开添加/编辑模态框
  const openModal = (record = null) => {
    setEditingRecord(record)
    if (record) {
      form.setFieldsValue({
        ...record,
        outboundDate: record.outboundDate ? dayjs(record.outboundDate) : null,
        items: record.items || []
      })
    } else {
      form.resetFields()
      form.setFieldsValue({
        outboundOrderNumber: generateOutboundOrderNumber(),
        outboundDate: null,
        items: []
      })
    }
    setModalVisible(true)
  }

  // 打开查看模态框
  const openViewModal = (record) => {
    setViewingRecord(record)
    setViewModalVisible(true)
  }

  // 关闭模态框
  const closeModal = () => {
    setModalVisible(false)
    setEditingRecord(null)
    form.resetFields()
  }

  // 关闭查看模态框
  const closeViewModal = () => {
    setViewModalVisible(false)
    setViewingRecord(null)
  }

  // 保存耗材出库
  const saveConsumableOutbound = async () => {
    try {
      const values = await form.validateFields()
      message.success(editingRecord ? '更新耗材出库成功' : '创建耗材出库成功')
      closeModal()
      loadConsumableOutbounds()
    } catch (error) {
      message.error('保存失败，请检查输入')
    }
  }

  // 删除耗材出库
  const deleteConsumableOutbound = async (id) => {
    try {
      message.success('删除耗材出库成功')
      loadConsumableOutbounds()
    } catch (error) {
      message.error('删除失败')
    }
  }





  // 表格列配置
  const columns = [
    {
      title: '出库单号',
      dataIndex: 'outboundOrderNumber',
      key: 'outboundOrderNumber'
    },
    {
      title: '出库日期',
      dataIndex: 'outboundDate',
      key: 'outboundDate'
    },
    {
      title: '出库说明',
      dataIndex: 'outboundDescription',
      key: 'outboundDescription'
    },
    {
      title: '总数量',
      dataIndex: 'totalQuantity',
      key: 'totalQuantity'
    },
    {
      title: '总金额',
      dataIndex: 'totalAmount',
      key: 'totalAmount',
      render: (text) => `¥${text}`
    },
    {
      title: '接收人',
      dataIndex: 'receiver',
      key: 'receiver'
    },
    {
      title: '操作人',
      dataIndex: 'operator',
      key: 'operator'
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
            icon={<EyeOutlined />}
            onClick={() => openViewModal(record)}
          >
            预览
          </Button>
          <Button
            type="primary"
            icon={<EditOutlined />}
            onClick={() => openModal(record)}
          >
            编辑
          </Button>
          <Popconfirm
            title="确定要删除这个耗材出库吗？"
            onConfirm={() => deleteConsumableOutbound(record.id)}
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
        <h2>耗材出库管理</h2>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => openModal()}>
          + 新增耗材出库
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
        title={editingRecord ? '编辑耗材出库单' : '新增耗材出库单'}
        open={modalVisible}
        onCancel={closeModal}
        onOk={saveConsumableOutbound}
        width={800}
      >
        <Form form={form} layout="vertical">
          <div style={{ display: 'flex', gap: 16, marginBottom: 16 }}>
            <Form.Item
              name="outboundOrderNumber"
              label="出库单号"
              style={{ flex: 1 }}
            >
              <Input placeholder="出库单号" disabled />
            </Form.Item>
            <Form.Item
              name="outboundDate"
              label="出库日期"
              rules={[{ required: true, message: '请选择出库日期' }]}
              style={{ flex: 1 }}
            >
              <DatePicker style={{ width: '100%' }} />
            </Form.Item>
          </div>

          <Form.Item
            name="outboundDescription"
            label="出库说明"
            rules={[{ required: true, message: '请输入出库说明' }]}
          >
            <Input placeholder="请输入出库说明" />
          </Form.Item>

          <Form.Item label="出库明细">
            <Form.List name="items">
              {(fields, { add, remove }) => (
                <div>
                  <div style={{ display: 'flex', marginBottom: 8, fontWeight: 'bold' }}>
                    <div style={{ flex: 2 }}>耗材</div>
                    <div style={{ flex: 1 }}>规格</div>
                    <div style={{ flex: 1 }}>数量</div>
                    <div style={{ flex: 1 }}>单位</div>
                    <div style={{ flex: 1 }}>单价</div>
                    <div style={{ flex: 1 }}>可用数量</div>
                    <div style={{ flex: 1 }}>备注</div>
                    <div style={{ flex: 0.5 }}>操作</div>
                  </div>
                  {fields.map((field, index) => (
                    <div key={field.key} style={{ display: 'flex', gap: 8, marginBottom: 8, alignItems: 'flex-start' }}>
                      <Form.Item
                        {...field}
                        name={[field.name, 'consumableId']}
                        fieldKey={[field.fieldKey, 'consumableId']}
                        rules={[{ required: true, message: '请选择耗材' }]}
                        style={{ flex: 2, marginBottom: 0 }}
                      >
                        <Select placeholder="请选择耗材">
                          {consumables.map(consumable => (
                            <Option key={consumable.id} value={consumable.id}>
                              {consumable.name}
                            </Option>
                          ))}
                        </Select>
                      </Form.Item>
                      <Form.Item
                        {...field}
                        name={[field.name, 'specification']}
                        fieldKey={[field.fieldKey, 'specification']}
                        style={{ flex: 1, marginBottom: 0 }}
                      >
                        <Input placeholder="规格" />
                      </Form.Item>
                      <Form.Item
                        {...field}
                        name={[field.name, 'quantity']}
                        fieldKey={[field.fieldKey, 'quantity']}
                        rules={[{ required: true, message: '请输入数量' }, { type: 'number', min: 1 }]}
                        style={{ flex: 1, marginBottom: 0 }}
                      >
                        <Input type="number" placeholder="1" />
                      </Form.Item>
                      <Form.Item
                        {...field}
                        name={[field.name, 'unit']}
                        fieldKey={[field.fieldKey, 'unit']}
                        rules={[{ required: true, message: '请输入单位' }]}
                        style={{ flex: 1, marginBottom: 0 }}
                      >
                        <Input placeholder="单位" />
                      </Form.Item>
                      <Form.Item
                        {...field}
                        name={[field.name, 'price']}
                        fieldKey={[field.fieldKey, 'price']}
                        rules={[{ required: true, message: '请输入单价' }, { type: 'number', min: 0 }]}
                        style={{ flex: 1, marginBottom: 0 }}
                      >
                        <Input type="number" placeholder="0" />
                      </Form.Item>
                      <Form.Item
                        {...field}
                        name={[field.name, 'availableQuantity']}
                        fieldKey={[field.fieldKey, 'availableQuantity']}
                        style={{ flex: 1, marginBottom: 0 }}
                      >
                        <Input placeholder="可用0" disabled />
                      </Form.Item>
                      <Form.Item
                        {...field}
                        name={[field.name, 'remark']}
                        fieldKey={[field.fieldKey, 'remark']}
                        style={{ flex: 1, marginBottom: 0 }}
                      >
                        <Input placeholder="备注" />
                      </Form.Item>
                      <Form.Item style={{ flex: 0.5, marginBottom: 0 }}>
                        <Button danger onClick={() => remove(index)}>
                          <DeleteOutlined />
                        </Button>
                      </Form.Item>
                    </div>
                  ))}
                  <Button type="dashed" onClick={() => add({})} style={{ width: '100%' }}>
                    <PlusOutlined /> 添加明细行
                  </Button>
                </div>
              )}
            </Form.List>
          </Form.Item>

          <div style={{ display: 'flex', gap: 16, marginBottom: 16 }}>
            <Form.Item
              name="receiver"
              label="接收人"
              rules={[{ required: true, message: '请输入接收人' }]}
              style={{ flex: 1 }}
            >
              <Input placeholder="请输入接收人" />
            </Form.Item>
            <Form.Item
              name="operator"
              label="操作人"
              rules={[{ required: true, message: '请输入操作人' }]}
              style={{ flex: 1 }}
            >
              <Input placeholder="请输入操作人" />
            </Form.Item>
          </div>

          <Form.Item
            name="remark"
            label="出库备注"
          >
            <TextArea rows={3} placeholder="请输入出库备注（可选）" />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title="耗材出库单详情"
        open={viewModalVisible}
        onCancel={closeViewModal}
        footer={[
          <Button key="close" onClick={closeViewModal}>
            关闭
          </Button>
        ]}
        width={800}
      >
        {viewingRecord && (
          <div>
            <div style={{ display: 'flex', gap: 16, marginBottom: 16 }}>
              <div style={{ flex: 1 }}>
                <div style={{ marginBottom: 8 }}>
                  <span style={{ fontWeight: 'bold' }}>出库单号：</span>
                  {viewingRecord.outboundOrderNumber}
                </div>
                <div style={{ marginBottom: 8 }}>
                  <span style={{ fontWeight: 'bold' }}>出库日期：</span>
                  {viewingRecord.outboundDate}
                </div>
              </div>
              <div style={{ flex: 1 }}>
                <div style={{ marginBottom: 8 }}>
                  <span style={{ fontWeight: 'bold' }}>接收人：</span>
                  {viewingRecord.receiver}
                </div>
                <div style={{ marginBottom: 8 }}>
                  <span style={{ fontWeight: 'bold' }}>操作人：</span>
                  {viewingRecord.operator}
                </div>
              </div>
            </div>
            <div style={{ marginBottom: 16 }}>
              <span style={{ fontWeight: 'bold' }}>出库说明：</span>
              {viewingRecord.outboundDescription}
            </div>
            <div style={{ marginBottom: 16 }}>
              <span style={{ fontWeight: 'bold' }}>出库明细：</span>
              <div style={{ marginTop: 8, border: '1px solid #f0f0f0', borderRadius: 4, overflow: 'hidden' }}>
                <div style={{ display: 'flex', backgroundColor: '#fafafa', padding: 8, fontWeight: 'bold' }}>
                  <div style={{ flex: 2 }}>耗材</div>
                  <div style={{ flex: 1 }}>规格</div>
                  <div style={{ flex: 1 }}>数量</div>
                  <div style={{ flex: 1 }}>单位</div>
                  <div style={{ flex: 1 }}>单价</div>
                  <div style={{ flex: 1 }}>可用数量</div>
                  <div style={{ flex: 1 }}>备注</div>
                </div>
                {viewingRecord.items.map((item, index) => (
                  <div key={item.id} style={{ display: 'flex', padding: 8, borderTop: '1px solid #f0f0f0' }}>
                    <div style={{ flex: 2 }}>{item.consumableName}</div>
                    <div style={{ flex: 1 }}>{item.specification}</div>
                    <div style={{ flex: 1 }}>{item.quantity}</div>
                    <div style={{ flex: 1 }}>{item.unit}</div>
                    <div style={{ flex: 1 }}>¥{item.price}</div>
                    <div style={{ flex: 1 }}>{item.availableQuantity}</div>
                    <div style={{ flex: 1 }}>{item.remark || '-'}</div>
                  </div>
                ))}
              </div>
            </div>
            <div style={{ marginBottom: 16 }}>
              <span style={{ fontWeight: 'bold' }}>总金额：</span>
              ¥{viewingRecord.totalAmount}
            </div>
            <div>
              <span style={{ fontWeight: 'bold' }}>出库备注：</span>
              {viewingRecord.remark || '-'}
            </div>
          </div>
        )}
      </Modal>
    </div>
  )
}

export default ConsumableOutbound