import React, { useState, useEffect } from 'react'
import { Table, Button, Modal, Form, Input, Select, DatePicker, message, Space, Popconfirm } from 'antd'
import { PlusOutlined, EditOutlined, DeleteOutlined, SearchOutlined, ReloadOutlined } from '@ant-design/icons'
import dayjs from 'dayjs'

const { Option } = Select
const { TextArea } = Input

function RepairEquipmentManagement() {
  const [form] = Form.useForm()
  const [dataSource, setDataSource] = useState([])
  const [loading, setLoading] = useState(false)
  const [modalVisible, setModalVisible] = useState(false)
  const [editingRecord, setEditingRecord] = useState(null)
  const [currentPage, setCurrentPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [total, setTotal] = useState(0)

  // 设备类型（模拟数据）
  const equipmentTypes = [
    { id: 1, name: '专用设备' },
    { id: 2, name: '通用设备' }
  ]

  // 维修状态（模拟数据）
  const repairStatuses = [
    { id: 1, name: '待维修' },
    { id: 2, name: '维修中' },
    { id: 3, name: '已完成' }
  ]

  // 加载待维修设备数据
  const loadRepairEquipments = async () => {
    setLoading(true)
    try {
      // 模拟API调用
      const mockData = {
        items: [
          {
            id: 1,
            equipmentName: '设备1',
            model: 'Model-001',
            equipmentType: '专用设备',
            faultDescription: '无法开机',
            repairStatus: '待维修',
            submitDate: '2026-03-15',
            remark: '需要更换电源'
          },
          {
            id: 2,
            equipmentName: '设备2',
            model: 'Model-002',
            equipmentType: '通用设备',
            faultDescription: '屏幕闪烁',
            repairStatus: '维修中',
            submitDate: '2026-03-16',
            remark: '需要更换屏幕'
          },
          {
            id: 3,
            equipmentName: '设备3',
            model: 'Model-003',
            equipmentType: '专用设备',
            faultDescription: '网络连接失败',
            repairStatus: '已完成',
            submitDate: '2026-03-14',
            remark: '已更换网络模块'
          }
        ],
        totalCount: 3
      }
      setDataSource(mockData.items)
      setTotal(mockData.totalCount)
    } catch (error) {
      message.error('加载待维修设备数据失败')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadRepairEquipments()
  }, [currentPage, pageSize])

  // 打开添加/编辑模态框
  const openModal = (record = null) => {
    setEditingRecord(record)
    if (record) {
      form.setFieldsValue({
        ...record,
        submitDate: record.submitDate ? dayjs(record.submitDate) : null
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

  // 保存待维修设备
  const saveRepairEquipment = async () => {
    try {
      const values = await form.validateFields()
      message.success(editingRecord ? '更新待维修设备成功' : '创建待维修设备成功')
      closeModal()
      loadRepairEquipments()
    } catch (error) {
      message.error('保存失败，请检查输入')
    }
  }

  // 删除待维修设备
  const deleteRepairEquipment = async (id) => {
    try {
      message.success('删除待维修设备成功')
      loadRepairEquipments()
    } catch (error) {
      message.error('删除失败')
    }
  }

  // 表格列配置
  const columns = [
    {
      title: '设备名称',
      dataIndex: 'equipmentName',
      key: 'equipmentName'
    },
    {
      title: '型号',
      dataIndex: 'model',
      key: 'model'
    },
    {
      title: '设备类型',
      dataIndex: 'equipmentType',
      key: 'equipmentType'
    },
    {
      title: '故障描述',
      dataIndex: 'faultDescription',
      key: 'faultDescription'
    },
    {
      title: '维修状态',
      dataIndex: 'repairStatus',
      key: 'repairStatus'
    },
    {
      title: '提交日期',
      dataIndex: 'submitDate',
      key: 'submitDate'
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
            title="确定要删除这个待维修设备吗？"
            onConfirm={() => deleteRepairEquipment(record.id)}
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
        <h2>待维修设备管理</h2>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => openModal()}>
          添加待维修设备
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
        title={editingRecord ? '编辑待维修设备' : '添加待维修设备'}
        open={modalVisible}
        onCancel={closeModal}
        onOk={saveRepairEquipment}
        width={600}
      >
        <Form form={form} layout="vertical">
          <Form.Item
            name="equipmentName"
            label="设备名称"
            rules={[{ required: true, message: '请输入设备名称' }]}
          >
            <Input placeholder="请输入设备名称" />
          </Form.Item>

          <Form.Item
            name="model"
            label="型号"
            rules={[{ required: true, message: '请输入型号' }]}
          >
            <Input placeholder="请输入型号" />
          </Form.Item>

          <Form.Item
            name="equipmentType"
            label="设备类型"
            rules={[{ required: true, message: '请选择设备类型' }]}
          >
            <Select placeholder="请选择设备类型">
              {equipmentTypes.map(type => (
                <Option key={type.id} value={type.name}>
                  {type.name}
                </Option>
              ))}
            </Select>
          </Form.Item>

          <Form.Item
            name="faultDescription"
            label="故障描述"
            rules={[{ required: true, message: '请输入故障描述' }]}
          >
            <TextArea rows={4} placeholder="请输入故障描述" />
          </Form.Item>

          <Form.Item
            name="repairStatus"
            label="维修状态"
            rules={[{ required: true, message: '请选择维修状态' }]}
          >
            <Select placeholder="请选择维修状态">
              {repairStatuses.map(status => (
                <Option key={status.id} value={status.name}>
                  {status.name}
                </Option>
              ))}
            </Select>
          </Form.Item>

          <Form.Item
            name="submitDate"
            label="提交日期"
            rules={[{ required: true, message: '请选择提交日期' }]}
          >
            <DatePicker style={{ width: '100%' }} />
          </Form.Item>

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

export default RepairEquipmentManagement