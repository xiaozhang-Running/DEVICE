import React, { useState, useEffect } from 'react'
import { Card, Table, Button, Input, Select, Space, message, Modal, Form, Upload, Image, Popconfirm, Tag, InputNumber } from 'antd'
import { SearchOutlined, EditOutlined, DeleteOutlined, PlusOutlined, UploadOutlined, DownloadOutlined } from '@ant-design/icons'
import specialEquipmentApi from '../api/specialEquipment'
import usePermission from '../hooks/usePermission'
import './DeviceManagement.css'

const { Option } = Select
const { Search } = Input

const DeviceManagement = () => {
  const [devices, setDevices] = useState([])
  const [loading, setLoading] = useState(false)
  const [searchKeyword, setSearchKeyword] = useState('')
  const [deviceStatus, setDeviceStatus] = useState(null)
  const [useStatus, setUseStatus] = useState(null)
  const [brand, setBrand] = useState('')
  const [modalVisible, setModalVisible] = useState(false)
  const [importModalVisible, setImportModalVisible] = useState(false)
  const [editingDevice, setEditingDevice] = useState(null)
  const [form] = Form.useForm()
  const [imageUrl, setImageUrl] = useState('')
  const [currentPage, setCurrentPage] = useState(1)
  const [pageSize, setPageSize] = useState(20)
  const [total, setTotal] = useState(0)
  const [isMounted, setIsMounted] = useState(true)
  const { canAction } = usePermission()

  // 组件卸载时设置isMounted为false
  useEffect(() => {
    return () => {
      setIsMounted(false)
    }
  }, [])

  // 设备状态选项
  const deviceStatusOptions = [
    { value: 1, label: '正常' },
    { value: 2, label: '维修中' },
    { value: 3, label: '报废' }
  ]

  // 使用状态选项
  const useStatusOptions = [
    { value: 1, label: '使用中' },
    { value: 2, label: '未使用' },
    { value: 3, label: '已借出' }
  ]

  // 品牌选项（从设备数据中提取）
  const brandOptions = Array.isArray(devices) ? [...new Set(devices.map(device => device.brand).filter(Boolean))] : []

  // 获取设备列表
  const fetchDevices = async (page = 1, size = 20) => {
    if (!isMounted) return // 组件已卸载，直接返回
    setLoading(true)
    try {
      const data = await specialEquipmentApi.getAll({
        pageNumber: page,
        pageSize: size,
        keyword: searchKeyword,
        deviceStatus: deviceStatus,
        useStatus: useStatus,
        brand: brand
      })
      if (!isMounted) return // 组件已卸载，直接返回
      // 检查响应格式并正确设置设备数据
      if (data && data.success && data.data) {
        if (data.data.items && Array.isArray(data.data.items)) {
          setDevices(data.data.items)
          setTotal(data.data.total || 0)
        } else if (Array.isArray(data.data)) {
          setDevices(data.data)
          setTotal(data.data.length)
        } else {
          setDevices([])
          setTotal(0)
        }
      } else {
        setDevices([])
        setTotal(0)
      }
    } catch (error) {
      if (!isMounted) return // 组件已卸载，直接返回
      message.error('获取设备列表失败')
      console.error('Error fetching devices:', error)
      setDevices([])
      setTotal(0)
    } finally {
      if (isMounted) {
        setLoading(false)
      }
    }
  }

  // 初始加载
  useEffect(() => {
    fetchDevices(currentPage, pageSize)
  }, [searchKeyword, deviceStatus, useStatus, brand, currentPage, pageSize])

  // 处理搜索
  const handleSearch = (value) => {
    setSearchKeyword(value)
    setCurrentPage(1)
  }

  // 处理添加设备
  const handleAddDevice = () => {
    setEditingDevice(null)
    form.resetFields()
    setImageUrl('')
    setModalVisible(true)
  }

  // 处理编辑设备
  const handleEditDevice = (device) => {
    setEditingDevice(device)
    form.setFieldsValue({
      deviceName: device.deviceName,
      deviceCode: device.deviceCode,
      SN: device.SN,
      brand: device.brand,
      model: device.model,
      quantity: device.quantity || 1,
      unit: device.unit || '个',
      accessories: device.accessories,
      remark: device.remark,
      imageUrl: device.image || device.imageUrl,
      deviceStatus: device.deviceStatus,
      useStatus: device.useStatus,
      company: device.company,
      location: device.location
    })
    setImageUrl(device.image || device.imageUrl || '')
    setModalVisible(true)
  }

  // 处理删除设备
  const handleDeleteDevice = async (id) => {
    if (!isMounted) return // 组件已卸载，直接返回
    try {
      await specialEquipmentApi.delete(id)
      if (!isMounted) return // 组件已卸载，直接返回
      message.success('设备删除成功')
      fetchDevices(currentPage, pageSize)
    } catch (error) {
      if (!isMounted) return // 组件已卸载，直接返回
      message.error('设备删除失败')
      console.error('Error deleting device:', error)
    }
  }

  // 处理清空库存
  const handleClearInventory = async () => {
    if (!isMounted) return // 组件已卸载，直接返回
    try {
      await specialEquipmentApi.deleteAll()
      if (!isMounted) return // 组件已卸载，直接返回
      message.success('库存已清空')
      fetchDevices(currentPage, pageSize)
    } catch (error) {
      if (!isMounted) return // 组件已卸载，直接返回
      message.error('清空库存失败')
      console.error('Error clearing inventory:', error)
    }
  }

  // 处理表单提交
  const handleSubmit = async (values) => {
    if (!isMounted) return // 组件已卸载，直接返回
    try {
      if (editingDevice) {
        // 更新设备
        await specialEquipmentApi.update(editingDevice.id, values)
        if (!isMounted) return // 组件已卸载，直接返回
        message.success('设备更新成功')
      } else {
        // 创建设备
        await specialEquipmentApi.create(values)
        if (!isMounted) return // 组件已卸载，直接返回
        message.success('设备创建成功')
      }
      if (!isMounted) return // 组件已卸载，直接返回
      setModalVisible(false)
      fetchDevices(currentPage, pageSize)
    } catch (error) {
      if (!isMounted) return // 组件已卸载，直接返回
      message.error('操作失败')
      console.error('Error submitting form:', error)
    }
  }

  // 处理图片上传
  const handleImageUpload = (info) => {
    if (info.file.status === 'done') {
      // 假设上传成功后返回图片URL
      setImageUrl(info.file.response.url)
      form.setFieldsValue({ imageUrl: info.file.response.url })
    } else if (info.file.status === 'error') {
      message.error('图片上传失败')
    }
  }

  // 处理导入Excel
  const handleImportExcel = () => {
    setImportModalVisible(true)
  }

  // 处理导出Excel
  const handleExportExcel = () => {
    message.info('导出Excel功能开发中')
  }

  // 表格列定义
  const columns = [
    {
      title: '序号',
      dataIndex: 'index',
      key: 'index',
      width: 60,
      render: (_, __, index) => (currentPage - 1) * pageSize + index + 1
    },
    {
      title: '设备名称',
      dataIndex: 'deviceName',
      key: 'deviceName',
      width: 150
    },
    {
      title: '设备编号',
      dataIndex: 'deviceCode',
      key: 'deviceCode',
      width: 120
    },
    {
      title: 'SN码',
      dataIndex: 'SN',
      key: 'SN',
      width: 100
    },
    {
      title: '品牌',
      dataIndex: 'brand',
      key: 'brand',
      width: 100
    },
    {
      title: '型号',
      dataIndex: 'model',
      key: 'model',
      width: 100
    },
    {
      title: '数量',
      dataIndex: 'quantity',
      key: 'quantity',
      width: 80
    },
    {
      title: '单位',
      dataIndex: 'unit',
      key: 'unit',
      width: 80
    },
    {
      title: '配件',
      dataIndex: 'accessories',
      key: 'accessories',
      width: 100
    },
    {
      title: '备注',
      dataIndex: 'remark',
      key: 'remark',
      width: 100
    },
    {
      title: '图片',
      dataIndex: 'imageUrl',
      key: 'imageUrl',
      width: 80,
      render: (image) => image ? '有' : '-'
    },
    {
      title: '设备状态',
      dataIndex: 'deviceStatus',
      key: 'deviceStatus',
      width: 80,
      render: (status) => {
        const statusMap = {
          1: { label: '正常', color: 'green' },
          2: { label: '维修中', color: 'orange' },
          3: { label: '报废', color: 'red' }
        }
        const statusInfo = statusMap[status] || { label: '未知', color: 'gray' }
        return <Tag color={statusInfo.color}>{statusInfo.label}</Tag>
      }
    },
    {
      title: '使用状态',
      dataIndex: 'useStatus',
      key: 'useStatus',
      width: 80,
      render: (status) => {
        const statusMap = {
          1: { label: '使用中', color: 'blue' },
          2: { label: '未使用', color: 'green' },
          3: { label: '已借出', color: 'orange' }
        }
        const statusInfo = statusMap[status] || { label: '未知', color: 'gray' }
        return <Tag color={statusInfo.color}>{statusInfo.label}</Tag>
      }
    },
    {
      title: '所属公司',
      dataIndex: 'company',
      key: 'company',
      width: 120
    },
    {
      title: '所在仓库',
      dataIndex: 'location',
      key: 'location',
      width: 120
    },
    {
      title: '操作',
      key: 'action',
      width: 100,
      render: (_, record) => {
        return (
          <Space size="middle">
            <Button type="link" icon={<EditOutlined />} onClick={() => handleEditDevice(record)} />
            <Popconfirm
              title="确定要删除这个设备吗？"
              onConfirm={() => handleDeleteDevice(record.id)}
              okText="确定"
              cancelText="取消"
            >
              <Button type="link" danger icon={<DeleteOutlined />} />
            </Popconfirm>
          </Space>
        )
      }
    }
  ]

  return (
    <div className="device-management">
      <Card
        title="专用设备管理"
        extra={
          <Space>
            <Search
              placeholder="搜索设备"
              value={searchKeyword}
              onChange={(e) => setSearchKeyword(e.target.value)}
              onSearch={handleSearch}
              style={{ width: 200 }}
              allowClear
            />
            <Select
              placeholder="使用状态"
              value={useStatus}
              onChange={setUseStatus}
              style={{ width: 100 }}
              allowClear
            >
              {useStatusOptions.map(option => (
                <Option key={option.value} value={option.value}>{option.label}</Option>
              ))}
            </Select>
            <Select
              placeholder="品牌"
              value={brand}
              onChange={setBrand}
              style={{ width: 120 }}
              allowClear
            >
              {brandOptions.map(brand => (
                <Option key={brand} value={brand}>{brand}</Option>
              ))}
            </Select>
            <Button onClick={handleSearch}>筛选</Button>
            <Button icon={<UploadOutlined />} onClick={handleImportExcel}>导入Excel</Button>
            <Button icon={<DownloadOutlined />} onClick={handleExportExcel}>导出Excel</Button>
            <Button type="primary" icon={<PlusOutlined />} onClick={handleAddDevice}>新增设备</Button>
            <Popconfirm
              title="确定要清空所有设备吗？"
              onConfirm={handleClearInventory}
              okText="确定"
              cancelText="取消"
            >
              <Button danger icon={<DeleteOutlined />}>清空库存</Button>
            </Popconfirm>
          </Space>
        }
      >
        <Table
          columns={columns}
          dataSource={devices}
          rowKey={(record) => record.id || Math.random().toString(36)}
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
          scroll={{ x: 1800 }}
        />
      </Card>

      {/* 导入Excel模态框 */}
      <Modal
        title="导入Excel文件"
        open={importModalVisible}
        onCancel={() => setImportModalVisible(false)}
        footer={[
          <Button key="cancel" onClick={() => setImportModalVisible(false)}>取消</Button>,
          <Button key="import" type="primary" onClick={() => {
            message.success('导入成功')
            setImportModalVisible(false)
            fetchDevices(currentPage, pageSize)
          }}>开始导入</Button>
        ]}
        width={600}
      >
        <div style={{ marginBottom: 20 }}>
          <Button type="primary" icon={<UploadOutlined />}>导入Excel</Button>
          <Button style={{ marginLeft: 10 }} icon={<DownloadOutlined />}>下载模板</Button>
        </div>
        <div style={{ marginBottom: 20 }}>
          <p>请上传包含专用设备信息的Excel文件，支持xlsx和xls格式</p>
          <p style={{ marginTop: 10, fontSize: '12px', color: '#666' }}>表格格式要求：</p>
          <ul style={{ marginTop: 5, fontSize: '12px', color: '#666' }}>
            <li>必须包含列：设备名称、设备编号、配件、备注、所属公司、所在仓库、设备状态、使用状态</li>
            <li>可选项：SN码、品牌、型号、数量、单位、配件、备注</li>
            <li>设备状态：正常/维修中/报废</li>
            <li>使用状态：未使用/使用中</li>
          </ul>
        </div>
      </Modal>

      {/* 添加/编辑设备模态框 */}
      <Modal
        title={editingDevice ? '编辑设备' : '新增设备'}
        open={modalVisible}
        onCancel={() => setModalVisible(false)}
        footer={null}
        width={800}
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleSubmit}
        >
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16 }}>
            <Form.Item
              name="deviceType"
              label="设备类型"
              initialValue="专用设备"
            >
              <Select disabled>
                <Option value="专用设备">专用设备</Option>
              </Select>
            </Form.Item>
            <Form.Item
              name="deviceName"
              label="设备名称"
              rules={[{ required: true, message: '请输入设备名称' }]}
            >
              <Input placeholder="请输入设备名称" />
            </Form.Item>
            <Form.Item
              name="deviceCode"
              label="设备编号"
              rules={[{ required: true, message: '请输入设备编号' }]}
            >
              <Input placeholder="请输入设备编号" />
            </Form.Item>
            <Form.Item
              name="SN"
              label="SN码"
            >
              <Input placeholder="请输入SN码" />
            </Form.Item>
            <Form.Item
              name="brand"
              label="品牌"
            >
              <Input placeholder="请输入品牌" />
            </Form.Item>
            <Form.Item
              name="model"
              label="型号"
            >
              <Input placeholder="请输入型号" />
            </Form.Item>
            <Form.Item
              name="quantity"
              label="数量"
              initialValue={1}
            >
              <InputNumber min={1} style={{ width: '100%' }} />
            </Form.Item>
            <Form.Item
              name="unit"
              label="单位"
              initialValue="个"
            >
              <Input placeholder="如：台、个" />
            </Form.Item>
            <Form.Item
              name="deviceStatus"
              label="设备状态"
              initialValue={1}
              rules={[{ required: true, message: '请选择设备状态' }]}
            >
              <Select>
                {deviceStatusOptions.map(option => (
                  <Option key={option.value} value={option.value}>{option.label}</Option>
                ))}
              </Select>
            </Form.Item>
            <Form.Item
              name="useStatus"
              label="使用状态"
              initialValue={2}
              rules={[{ required: true, message: '请选择使用状态' }]}
            >
              <Select>
                {useStatusOptions.map(option => (
                  <Option key={option.value} value={option.value}>{option.label}</Option>
                ))}
              </Select>
            </Form.Item>
            <Form.Item
              name="company"
              label="所属公司"
            >
              <Input placeholder="请输入所属公司" />
            </Form.Item>
            <Form.Item
              name="location"
              label="所在仓库"
            >
              <Input placeholder="请输入所在仓库" />
            </Form.Item>
            <Form.Item
              name="accessories"
              label="配件"
              span={2}
            >
              <Input placeholder="请输入配件信息" />
            </Form.Item>
            <Form.Item
              name="remark"
              label="备注"
              span={2}
            >
              <Input.TextArea placeholder="请输入备注" />
            </Form.Item>
            <Form.Item
              name="imageUrl"
              label="图片"
              span={2}
            >
              <Upload
                name="file"
                action="/api/upload"
                listType="picture"
                onChange={handleImageUpload}
                fileList={imageUrl ? [{ url: imageUrl }] : []}
              >
                <Button icon={<PlusOutlined />}>上传图片</Button>
              </Upload>
            </Form.Item>
          </div>
          <Form.Item style={{ marginTop: 20 }}>
            <Space style={{ float: 'right' }}>
              <Button onClick={() => setModalVisible(false)}>Cancel</Button>
              <Button type="primary" htmlType="submit">
                {editingDevice ? 'OK' : 'OK'}
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  )
}

export default DeviceManagement