import React, { useState, useEffect } from 'react'
import { Table, Button, Modal, Form, Input, Select, DatePicker, message, Space, Upload, Tag, Card } from 'antd'
import { PlusOutlined, SearchOutlined } from '@ant-design/icons'
import projectInboundApi from '../api/projectInbound'
import dayjs from 'dayjs'

const { Option } = Select
const { TextArea } = Input

function ProjectInbound() {
  const [form] = Form.useForm()
  const [dataSource, setDataSource] = useState([])
  const [loading, setLoading] = useState(false)
  const [modalVisible, setModalVisible] = useState(false)
  const [editingRecord, setEditingRecord] = useState(null)
  const [currentPage, setCurrentPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [total, setTotal] = useState(0)
  const [previewModalVisible, setPreviewModalVisible] = useState(false)
  const [previewRecord, setPreviewRecord] = useState(null)
  const [outbounds, setOutbounds] = useState([])
  const [selectedOutbound, setSelectedOutbound] = useState(null)
  const [searchKeyword, setSearchKeyword] = useState('')
  const [uninboundItems, setUninboundItems] = useState([])
  const [uploadedFiles, setUploadedFiles] = useState([])

  // 加载项目入库数据
  const loadProjectInbounds = async () => {
    setLoading(true)
    try {
      const response = await projectInboundApi.getProjectInbounds()
      if (response.data && response.data.success) {
        if (Array.isArray(response.data.data)) {
          setDataSource(response.data.data)
          setTotal(response.data.data.length)
        } else if (response.data.data.data && Array.isArray(response.data.data.data)) {
          setDataSource(response.data.data.data)
          setTotal(response.data.data.data.length)
        } else if (response.data.data.data && Array.isArray(response.data.data.data.items)) {
          setDataSource(response.data.data.data.items)
          setTotal(response.data.data.data.total || 0)
        } else {
          setDataSource([])
          setTotal(0)
        }
      } else {
        setDataSource([])
        setTotal(0)
      }
    } catch (error) {
      message.error('加载项目入库数据失败')
      console.error('Error loading project inbounds:', error)
      setDataSource([])
      setTotal(0)
    } finally {
      setLoading(false)
    }
  }

  // 加载可用的项目出库单
  const loadAvailableOutbounds = async () => {
    try {
      const response = await projectInboundApi.getAvailableProjectOutbounds()
      if (response.data && response.data.success && response.data.data) {
        setOutbounds(response.data.data)
      }
    } catch (error) {
      console.error('Error loading available outbounds:', error)
    }
  }

  // 加载未入库的项目出库项
  const loadUninboundItems = async (outboundId) => {
    try {
      const response = await projectInboundApi.getUninboundItemsByOutboundId(outboundId)
      if (response.data && response.data.success && response.data.data) {
        setUninboundItems(response.data.data)
      }
    } catch (error) {
      console.error('Error loading uninbound items:', error)
    }
  }

  // 处理文件上传
  const handleFileUpload = (file) => {
    // 实际项目中应该调用API上传文件
    setUploadedFiles([...uploadedFiles, file])
    return false
  }

  useEffect(() => {
    loadProjectInbounds()
  }, [currentPage, pageSize])

  // 打开添加/编辑模态框
  const openModal = (record = null) => {
    setEditingRecord(record)
    if (record) {
      form.setFieldsValue({
        ...record,
        inboundDate: record.inboundDate ? dayjs(record.inboundDate) : null
      })
    } else {
      // 生成入库单号
      const today = new Date()
      const year = today.getFullYear()
      const month = String(today.getMonth() + 1).padStart(2, '0')
      const day = String(today.getDate()).padStart(2, '0')
      const random = Math.floor(Math.random() * 10000).toString().padStart(4, '0')
      const inboundNumber = `XMKR${year}${month}${day}${random}`
      
      form.resetFields()
      form.setFieldsValue({
        inboundNumber,
        inboundDate: dayjs()
      })
    }
    loadAvailableOutbounds()
    setSelectedOutbound(null)
    setUninboundItems([])
    setUploadedFiles([])
    setModalVisible(true)
  }

  // 关闭模态框
  const closeModal = () => {
    setModalVisible(false)
    setEditingRecord(null)
    setSelectedOutbound(null)
    setUninboundItems([])
    setUploadedFiles([])
    form.resetFields()
  }

  // 关闭预览模态框
  const closePreviewModal = () => {
    setPreviewModalVisible(false)
    setPreviewRecord(null)
  }

  // 搜索出库单
  const searchOutbounds = async () => {
    // 实际项目中应该调用API进行搜索
    message.info('搜索功能开发中')
  }

  // 选择出库单
  const selectOutbound = (outbound) => {
    setSelectedOutbound(outbound)
    loadUninboundItems(outbound.id)
  }

  // 清除选择
  const clearSelection = () => {
    setSelectedOutbound(null)
    setUninboundItems([])
  }

  // 移除已选择的出库单
  const removeSelectedOutbound = () => {
    setSelectedOutbound(null)
    setUninboundItems([])
  }

  // 入库操作
  const handleInbound = (item) => {
    message.success('入库成功')
  }

  // 保存项目入库
  const saveProjectInbound = async () => {
    try {
      const values = await form.validateFields()
      
      // 转换日期格式
      const formattedValues = {
        ...values,
        inboundDate: values.inboundDate ? values.inboundDate.format('YYYY-MM-DD') : null,
        items: uninboundItems,
        files: uploadedFiles
      }
      
      if (editingRecord) {
        await projectInboundApi.updateProjectInbound(editingRecord.id, formattedValues)
        message.success('更新项目入库成功')
      } else {
        await projectInboundApi.createProjectInbound(formattedValues)
        message.success('创建项目入库成功')
      }
      
      closeModal()
      loadProjectInbounds()
    } catch (error) {
      message.error('保存失败，请检查输入')
      console.error('Error saving project inbound:', error)
    }
  }

  // 删除项目入库
  const deleteProjectInbound = async (id) => {
    try {
      await projectInboundApi.deleteProjectInbound(id)
      message.success('删除项目入库成功')
      loadProjectInbounds()
    } catch (error) {
      message.error('删除失败')
      console.error('Error deleting project inbound:', error)
    }
  }

  // 打开预览模态框
  const openPreviewModal = (record) => {
    setPreviewRecord(record)
    setPreviewModalVisible(true)
  }

  // 表格列配置
  const columns = [
    {
      title: '入库单号',
      dataIndex: 'inboundNumber',
      key: 'inboundNumber',
      width: 150
    },
    {
      title: '入库日期',
      dataIndex: 'inboundDate',
      key: 'inboundDate',
      width: 120
    },
    {
      title: '项目名称',
      dataIndex: 'projectName',
      key: 'projectName',
      width: 150
    },
    {
      title: '项目负责人',
      dataIndex: 'projectManager',
      key: 'projectManager',
      width: 120
    },
    {
      title: '使用地',
      dataIndex: 'usageLocation',
      key: 'usageLocation',
      width: 120
    },
    {
      title: '经办人',
      dataIndex: 'operator',
      key: 'operator',
      width: 100
    },
    {
      title: '库管',
      dataIndex: 'warehouseManager',
      key: 'warehouseManager',
      width: 100
    },
    {
      title: '总数量',
      dataIndex: 'totalQuantity',
      key: 'totalQuantity',
      width: 100
    },
    {
      title: '状态',
      dataIndex: 'status',
      key: 'status',
      width: 100,
      render: (status) => {
        let color = 'blue'
        let text = '待处理'
        if (status === 'completed') {
          color = 'green'
          text = '已完成'
        }
        return <Tag color={color}>{text}</Tag>
      }
    },
    {
      title: '操作',
      key: 'action',
      width: 200,
      render: (_, record) => (
        <Space size="middle">
          <Button type="primary" onClick={() => openModal(record)}>编辑</Button>
          <Button onClick={() => openPreviewModal(record)}>预览</Button>
          <Button danger onClick={() => deleteProjectInbound(record.id)}>删除</Button>
        </Space>
      )
    }
  ]

  return (
    <div>
      <Card
        title="项目入库管理"
        extra={
          <Button type="primary" icon={<PlusOutlined />} onClick={() => openModal()}>
            + 新建项目入库
          </Button>
        }
      >
        <Table
          columns={columns}
          dataSource={dataSource}
          rowKey="id"
          loading={loading}
          pagination={{
            current: currentPage,
            pageSize: pageSize,
            total: total,
            onChange: (page, size) => {
              setCurrentPage(page)
              setPageSize(size)
            },
            showSizeChanger: true,
            pageSizeOptions: ['10', '20', '50'],
            showTotal: (total) => `共 ${total} 条记录`
          }}
          scroll={{ x: 1200 }}
        />
      </Card>

      {/* 新增/编辑项目入库模态框 */}
      <Modal
        title={editingRecord ? '编辑项目入库' : '新增项目入库'}
        open={modalVisible}
        onCancel={closeModal}
        footer={[
          <Button key="preview" onClick={() => {
            form.validateFields().then(values => {
              setPreviewRecord({
                ...values,
                inboundDate: values.inboundDate ? values.inboundDate.format('YYYY-MM-DD') : null
              });
              setPreviewModalVisible(true);
            });
          }}>预览</Button>,
          <Button key="cancel" onClick={closeModal}>取消</Button>,
          <Button key="ok" type="primary" onClick={saveProjectInbound}>确定</Button>
        ]}
        width={1000}
      >
        <Form form={form} layout="vertical">
          <div style={{ marginBottom: 24 }}>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 2fr 1fr', gap: 16, marginBottom: 16 }}>
              <Form.Item
                name="inboundNumber"
                label="入库单号"
                rules={[{ required: true, message: '请输入入库单号' }]}
              >
                <Input placeholder="请输入入库单号" />
              </Form.Item>
              <Form.Item
                label="输入项目名称或出库单号"
              >
                <Input placeholder="输入项目名称或出库单号" value={searchKeyword} onChange={(e) => setSearchKeyword(e.target.value)} />
              </Form.Item>
              <Button type="primary" icon={<SearchOutlined />} onClick={searchOutbounds} style={{ marginTop: 24 }}>搜索</Button>
            </div>
            
            {selectedOutbound && (
              <div style={{ marginBottom: 16, padding: 12, border: '1px solid #e8e8e8', borderRadius: 4, backgroundColor: '#fafafa' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <div>
                    <strong>出库单号：{selectedOutbound.outboundNumber}</strong> 项目：{selectedOutbound.projectName} 日期：{selectedOutbound.outboundDate}
                  </div>
                  <Button danger size="small" onClick={removeSelectedOutbound}>删除</Button>
                </div>
              </div>
            )}
            
            <div style={{ marginBottom: 16 }}>
              <h4>搜索结果</h4>
              <Table
                columns={[
                  { title: '出库单号', dataIndex: 'outboundNumber', key: 'outboundNumber' },
                  { title: '项目名称', dataIndex: 'projectName', key: 'projectName' },
                  { title: '出库日期', dataIndex: 'outboundDate', key: 'outboundDate' },
                  { 
                    title: '操作', 
                    key: 'action',
                    render: (_, record) => (
                      <Button type="link" onClick={() => selectOutbound(record)}>选择</Button>
                    )
                  }
                ]}
                dataSource={outbounds}
                rowKey="id"
                pagination={{ pageSize: 5 }}
              />
            </div>
          </div>

          <div style={{ marginBottom: 24 }}>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr 1fr', gap: 16, marginBottom: 16 }}>
              <Form.Item
                name="projectName"
                label="项目名称"
                rules={[{ required: true, message: '请输入项目名称' }]}
              >
                <Input placeholder="请输入项目名称" />
              </Form.Item>
              <Form.Item
                name="projectManager"
                label="项目负责人"
                rules={[{ required: true, message: '请输入项目负责人' }]}
              >
                <Input placeholder="请输入项目负责人" />
              </Form.Item>
              <Form.Item
                name="projectDate"
                label="项目时间"
                rules={[{ required: true, message: '请输入项目时间' }]}
              >
                <Input placeholder="请输入项目时间" />
              </Form.Item>
              <Form.Item
                name="usageLocation"
                label="使用地"
                rules={[{ required: true, message: '请输入使用地' }]}
              >
                <Input placeholder="请输入使用地" />
              </Form.Item>
            </div>
          </div>

          <div style={{ marginBottom: 24 }}>
            <h4>入库图片</h4>
            <Upload.Dragger name="files" customRequest={handleFileUpload} listType="picture">
              <p className="ant-upload-drag-icon">
                <PlusOutlined />
              </p>
              <p className="ant-upload-text">点击或拖拽文件到此区域上传</p>
              <p className="ant-upload-hint">支持上传JPG、PNG等格式的图片</p>
            </Upload.Dragger>
          </div>

          <div style={{ marginBottom: 24 }}>
            <h4>入库物品</h4>
            <div style={{ marginBottom: 16 }}>
              <h5>已选物品</h5>
              <div style={{ marginBottom: 16 }}>
                <h6>专用设备</h6>
                <Table
                  columns={[
                    { title: '序号', dataIndex: 'index', key: 'index' },
                    { title: '出库单号', dataIndex: 'outboundNumber', key: 'outboundNumber' },
                    { title: '物品类型', dataIndex: 'itemType', key: 'itemType' },
                    { title: '物品名称', dataIndex: 'itemName', key: 'itemName' },
                    { title: '设备编号', dataIndex: 'deviceCode', key: 'deviceCode' },
                    { title: '品牌', dataIndex: 'brand', key: 'brand' },
                    { title: '型号', dataIndex: 'model', key: 'model' },
                    { title: '数量', dataIndex: 'quantity', key: 'quantity' },
                    { title: '单位', dataIndex: 'unit', key: 'unit' },
                    { title: '设备状态', dataIndex: 'status', key: 'status' },
                    { title: '备注', dataIndex: 'remark', key: 'remark' },
                    { title: '状态', dataIndex: 'inboundStatus', key: 'inboundStatus' },
                    { 
                      title: '操作', 
                      key: 'action',
                      render: (_, record) => (
                        <Button type="link" onClick={() => handleInbound(record)}>入库</Button>
                      )
                    }
                  ]}
                  dataSource={uninboundItems.filter(item => item.itemType === '专用设备').map((item, index) => ({ ...item, index: index + 1 }))}
                  rowKey="id"
                  pagination={false}
                />
              </div>
              
              <div style={{ marginBottom: 16 }}>
                <h6>通用设备</h6>
                <Table
                  columns={[
                    { title: '序号', dataIndex: 'index', key: 'index' },
                    { title: '出库单号', dataIndex: 'outboundNumber', key: 'outboundNumber' },
                    { title: '物品类型', dataIndex: 'itemType', key: 'itemType' },
                    { title: '物品名称', dataIndex: 'itemName', key: 'itemName' },
                    { title: '设备编号', dataIndex: 'deviceCode', key: 'deviceCode' },
                    { title: '品牌', dataIndex: 'brand', key: 'brand' },
                    { title: '型号', dataIndex: 'model', key: 'model' },
                    { title: '数量', dataIndex: 'quantity', key: 'quantity' },
                    { title: '单位', dataIndex: 'unit', key: 'unit' },
                    { title: '设备状态', dataIndex: 'status', key: 'status' },
                    { title: '备注', dataIndex: 'remark', key: 'remark' },
                    { title: '状态', dataIndex: 'inboundStatus', key: 'inboundStatus' },
                    { 
                      title: '操作', 
                      key: 'action',
                      render: (_, record) => (
                        <Button type="link" onClick={() => handleInbound(record)}>入库</Button>
                      )
                    }
                  ]}
                  dataSource={uninboundItems.filter(item => item.itemType === '通用设备').map((item, index) => ({ ...item, index: index + 1 }))}
                  rowKey="id"
                  pagination={false}
                />
              </div>
              
              <div style={{ marginBottom: 16 }}>
                <h6>耗材</h6>
                <Table
                  columns={[
                    { title: '序号', dataIndex: 'index', key: 'index' },
                    { title: '出库单号', dataIndex: 'outboundNumber', key: 'outboundNumber' },
                    { title: '物品类型', dataIndex: 'itemType', key: 'itemType' },
                    { title: '物品名称', dataIndex: 'itemName', key: 'itemName' },
                    { title: '设备编号', dataIndex: 'deviceCode', key: 'deviceCode' },
                    { title: '品牌', dataIndex: 'brand', key: 'brand' },
                    { title: '型号', dataIndex: 'model', key: 'model' },
                    { title: '数量', dataIndex: 'quantity', key: 'quantity' },
                    { title: '单位', dataIndex: 'unit', key: 'unit' },
                    { title: '设备状态', dataIndex: 'status', key: 'status' },
                    { title: '备注', dataIndex: 'remark', key: 'remark' },
                    { title: '状态', dataIndex: 'inboundStatus', key: 'inboundStatus' },
                    { 
                      title: '操作', 
                      key: 'action',
                      render: (_, record) => (
                        <Button type="link" onClick={() => handleInbound(record)}>入库</Button>
                      )
                    }
                  ]}
                  dataSource={uninboundItems.filter(item => item.itemType === '耗材').map((item, index) => ({ ...item, index: index + 1 }))}
                  rowKey="id"
                  pagination={false}
                />
              </div>
            </div>
          </div>

          <div style={{ marginBottom: 24 }}>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: 16, marginBottom: 16 }}>
              <Form.Item
                name="operator"
                label="经办人"
                rules={[{ required: true, message: '请输入经办人' }]}
              >
                <Input placeholder="请输入经办人" />
              </Form.Item>
              <Form.Item
                name="warehouseManager"
                label="库管"
                rules={[{ required: true, message: '请输入库管' }]}
              >
                <Input placeholder="请输入库管" />
              </Form.Item>
              <Form.Item
                name="inboundDate"
                label="入库日期"
                rules={[{ required: true, message: '请选择入库日期' }]}
              >
                <DatePicker style={{ width: '100%' }} />
              </Form.Item>
            </div>
          </div>

          <Form.Item
            name="remark"
            label="备注"
          >
            <TextArea rows={4} placeholder="请输入备注" />
          </Form.Item>
        </Form>
      </Modal>

      {/* 项目入库预览模态框 */}
      <Modal
        title="入库单预览"
        open={previewModalVisible}
        onCancel={closePreviewModal}
        footer={[
          <Button key="print" type="default">打印</Button>,
          <Button key="export" type="default">保存PDF</Button>,
          <Button key="close" type="primary" onClick={closePreviewModal}>关闭预览</Button>
        ]}
        width={800}
      >
        {previewRecord && (
          <div style={{ padding: 20 }}>
            <h2 style={{ textAlign: 'center', marginBottom: 30 }}>项目入库单</h2>
            <div style={{ marginBottom: 20 }}>
              <div style={{ marginBottom: 10 }}>
                <strong>入库单号：</strong>{previewRecord.inboundNumber}
              </div>
              <div style={{ marginBottom: 10 }}>
                <strong>状态：</strong>-</div>
              <div style={{ marginBottom: 10 }}>
                <strong>项目名称：</strong>{previewRecord.projectName || '-'}
              </div>
            </div>
            <div style={{ marginBottom: 20 }}>
              <div style={{ marginBottom: 10 }}>
                <strong>项目时间：</strong>{previewRecord.projectDate || '-'}
              </div>
              <div style={{ marginBottom: 10 }}>
                <strong>使用地：</strong>{previewRecord.usageLocation || '-'}
              </div>
              <div style={{ marginBottom: 10 }}>
                <strong>项目负责人：</strong>{previewRecord.projectManager || '-'}
              </div>
              <div style={{ marginBottom: 10 }}>
                <strong>联系电话：</strong>-</div>
            </div>
            <div style={{ marginBottom: 20 }}>
              <h3>入库物品</h3>
              <table style={{ width: '100%', borderCollapse: 'collapse', marginTop: 10 }}>
                <thead>
                  <tr style={{ borderBottom: '1px solid #ddd' }}>
                    <th style={{ padding: '8px', textAlign: 'left' }}>序号</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>物品类型</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>物品名称</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>设备编号</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>品牌</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>型号</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>数量</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>单位</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>设备状态</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>备注</th>
                  </tr>
                </thead>
                <tbody>
                  {uninboundItems.map((item, index) => (
                    <tr key={item.id || index} style={{ borderBottom: '1px solid #f0f0f0' }}>
                      <td style={{ padding: '8px' }}>{index + 1}</td>
                      <td style={{ padding: '8px' }}>{item.itemType}</td>
                      <td style={{ padding: '8px' }}>{item.itemName}</td>
                      <td style={{ padding: '8px' }}>{item.deviceCode || '-'}</td>
                      <td style={{ padding: '8px' }}>{item.brand || '-'}</td>
                      <td style={{ padding: '8px' }}>{item.model || '-'}</td>
                      <td style={{ padding: '8px' }}>{item.quantity}</td>
                      <td style={{ padding: '8px' }}>{item.unit}</td>
                      <td style={{ padding: '8px' }}>{item.status || '-'}</td>
                      <td style={{ padding: '8px' }}>{item.remark || '-'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            <div style={{ marginBottom: 20 }}>
              <h3>入库图片</h3>
              <div style={{ marginBottom: 10 }}>
                {uploadedFiles.length > 0 ? (
                  <div style={{ display: 'flex', gap: 10 }}>
                    {uploadedFiles.map((file, index) => (
                      <div key={file.url || file.uid || index} style={{ width: 100, height: 100, border: '1px solid #e8e8e8', borderRadius: 4, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                        <img src={file.url || URL.createObjectURL(file)} alt={`Uploaded ${index + 1}`} style={{ maxWidth: '100%', maxHeight: '100%' }} />
                      </div>
                    ))}
                  </div>
                ) : (
                  <strong>无图片</strong>
                )}
              </div>
            </div>
            <div style={{ marginBottom: 10 }}>
              <strong>经办人：</strong>{previewRecord.operator || '-'}
            </div>
            <div style={{ marginBottom: 10 }}>
              <strong>库管：</strong>{previewRecord.warehouseManager || '-'}
            </div>
            <div style={{ marginBottom: 10 }}>
              <strong>入库日期：</strong>{previewRecord.inboundDate}
            </div>
            <div style={{ marginBottom: 10 }}>
              <strong>备注：</strong>{previewRecord.remark || '-'}
            </div>
          </div>
        )}
      </Modal>
    </div>
  )
}

export default ProjectInbound