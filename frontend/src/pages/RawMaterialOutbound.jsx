import React, { useState, useEffect } from 'react'
import { Table, Button, Modal, Form, Input, Select, DatePicker, message, Space, Popconfirm, Tag, Card } from 'antd'
import { PlusOutlined, EditOutlined, DeleteOutlined, SearchOutlined, ReloadOutlined, EyeOutlined, FileTextOutlined, SaveOutlined } from '@ant-design/icons'
import dayjs from 'dayjs'
import rawMaterialOutboundApi from '../api/rawMaterialOutbound'
import rawMaterialApi from '../api/rawMaterial'

const { Option } = Select
const { TextArea } = Input

function RawMaterialOutbound() {
  const [form] = Form.useForm()
  const [dataSource, setDataSource] = useState([])
  const [loading, setLoading] = useState(false)
  const [modalVisible, setModalVisible] = useState(false)
  const [previewModalVisible, setPreviewModalVisible] = useState(false)
  const [editingRecord, setEditingRecord] = useState(null)
  const [previewRecord, setPreviewRecord] = useState(null)
  const [currentPage, setCurrentPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [total, setTotal] = useState(0)
  const [rawMaterials, setRawMaterials] = useState([])
  const [rawMaterialMap, setRawMaterialMap] = useState({})

  // 加载原料列表
  const loadRawMaterials = async () => {
    try {
      const response = await rawMaterialApi.getRawMaterials()
      if (response.success && response.data) {
        const materials = response.data || []
        setRawMaterials(materials)
        // 创建原料ID到原料对象的映射，使用字符串类型的id作为键，与Select组件的value类型匹配
        const map = {}
        materials.forEach(material => {
          map[String(material.id)] = material
        })
        setRawMaterialMap(map)
      }
    } catch (error) {
      console.error('加载原料列表失败:', error)
    }
  }

  // 加载原料出库数据
  const loadRawMaterialOutbounds = async () => {
    setLoading(true)
    try {
      const response = await rawMaterialOutboundApi.getRawMaterialOutbounds()
      if (response.Success && response.Data) {
        // 确保dataSource是数组
        if (Array.isArray(response.Data)) {
          setDataSource(response.Data)
          setTotal(response.Data.length)
        } else if (response.Data.data && Array.isArray(response.Data.data)) {
          setDataSource(response.Data.data)
          setTotal(response.Data.data.length)
        } else if (response.Data.data && Array.isArray(response.Data.data.items)) {
          setDataSource(response.Data.data.items)
          setTotal(response.Data.data.total || 0)
        } else {
          setDataSource([])
          setTotal(0)
        }
      } else {
        setDataSource([])
        setTotal(0)
      }
    } catch (error) {
      message.error('加载原料出库数据失败')
      console.error('Error loading raw material outbounds:', error)
      setDataSource([])
      setTotal(0)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadRawMaterials()
    loadRawMaterialOutbounds()
  }, [])

  // 生成出库单号
  const generateOutboundNumber = () => {
    const date = new Date()
    const year = date.getFullYear()
    const month = String(date.getMonth() + 1).padStart(2, '0')
    const day = String(date.getDate()).padStart(2, '0')
    const random = Math.floor(Math.random() * 10000).toString().padStart(4, '0')
    return `YCK${year}${month}${day}${random}`
  }

  // 打开添加/编辑模态框
  const openModal = (record = null) => {
    setEditingRecord(record)
    if (record) {
      form.setFieldsValue({
        OutboundNumber: record.OutboundNumber || record.OutboundOrderNumber,
        OutboundDate: record.OutboundDate ? dayjs(record.OutboundDate) : null,
        Recipient: record.Recipient || record.Receiver,
        Operator: record.Operator,
        Remark: record.Remark,
        Items: record.Items || []
      })
    } else {
      form.resetFields()
      form.setFieldsValue({
        OutboundNumber: generateOutboundNumber(),
        OutboundDate: dayjs(),
        Items: []
      })
    }
    setModalVisible(true)
  }

  // 打开预览模态框
  const openPreviewModal = async (values) => {
    setPreviewRecord(values)
    setPreviewModalVisible(true)
  }

  // 关闭模态框
  const closeModal = () => {
    setModalVisible(false)
    setEditingRecord(null)
    form.resetFields()
  }

  // 关闭预览模态框
  const closePreviewModal = () => {
    setPreviewModalVisible(false)
    setPreviewRecord(null)
  }

  // 保存原料出库
  const saveRawMaterialOutbound = async () => {
    try {
      const values = await form.validateFields()
      
      // 验证至少添加一个明细项
      if (!values.Items || values.Items.length === 0) {
        message.error('请至少添加一个出库明细项');
        return;
      }
      
      // 转换日期格式为ISO格式
      const formattedValues = {
        OutboundNumber: values.OutboundNumber,
        OutboundDate: values.OutboundDate ? values.OutboundDate.toISOString() : null,
        Recipient: values.Recipient || '',
        Operator: values.Operator || '',
        Remark: values.Remark || '',
        // 处理Items字段，确保数据结构与后端API匹配
        Items: values.Items.map(item => {
          // 检查是否是新创建的原材料（RawMaterialId不是数字）
          if (typeof item.RawMaterialId === 'string' && isNaN(Number(item.RawMaterialId))) {
            return {
              ProductName: item.RawMaterialId,
              Specification: item.Specification || '',
              Quantity: item.Quantity || 1,
              Remark: item.Remark || ''
            }
          }
          // 现有原材料
          const rawMaterialIdStr = String(item.RawMaterialId);
          const rawMaterialIdNum = typeof item.RawMaterialId === 'string' ? Number(item.RawMaterialId) : item.RawMaterialId;
          const material = rawMaterialMap[rawMaterialIdStr];
          if (!material) {
            // 如果找不到对应的原材料，不发送RawMaterialId字段，只发送ProductName字段
            // 并且确保ProductName不是数字字符串
            return {
              ProductName: `原材料 ${item.RawMaterialId}`,
              Specification: item.Specification || '',
              Quantity: item.Quantity || 1,
              Remark: item.Remark || ''
            };
          }
          return {
            RawMaterialId: rawMaterialIdNum,
            ProductName: material.productName,
            Specification: item.Specification || '',
            Quantity: item.Quantity || 1,
            Remark: item.Remark || ''
          }
        })
      }
      
      // 调试：打印发送到后端的数据
      console.log('Sending data to backend:', formattedValues);
      console.log('Items data:', formattedValues.Items);
      
      if (editingRecord) {
        await rawMaterialOutboundApi.updateRawMaterialOutbound(editingRecord.Id, formattedValues)
        message.success('更新原料出库成功')
      } else {
        await rawMaterialOutboundApi.createRawMaterialOutbound(formattedValues)
        message.success('创建原料出库成功')
      }
      
      closeModal()
      loadRawMaterialOutbounds()
    } catch (error) {
      message.error('保存失败，请检查输入')
      console.error('Error saving raw material outbound:', error)
    }
  }

  // 删除原料出库
  const deleteRawMaterialOutbound = async (id) => {
    try {
      await rawMaterialOutboundApi.deleteRawMaterialOutbound(id)
      message.success('删除原料出库成功')
      loadRawMaterialOutbounds()
    } catch (error) {
      message.error('删除失败')
      console.error('Error deleting raw material outbound:', error)
    }
  }



  // 表格列配置
  const columns = [
    {
      title: '出库单号',
      dataIndex: 'OutboundNumber',
      key: 'OutboundNumber',
      width: 150,
      render: (text, record) => record.OutboundNumber || record.OutboundOrderNumber
    },
    {
      title: '出库日期',
      dataIndex: 'OutboundDate',
      key: 'OutboundDate',
      width: 120
    },
    {
      title: '总数量',
      dataIndex: 'TotalQuantity',
      key: 'TotalQuantity',
      width: 80
    },
    {
      title: '收货人',
      dataIndex: 'Recipient',
      key: 'Recipient',
      width: 100,
      render: (text, record) => record.Recipient || record.Receiver
    },
    {
      title: '操作人',
      dataIndex: 'Operator',
      key: 'Operator',
      width: 100
    },
    {
      title: '备注',
      dataIndex: 'Remark',
      key: 'Remark',
      width: 150
    },
    {
      title: '操作',
      key: 'action',
      width: 100,
      render: (_, record) => (
        <Space size="middle">
          <Button type="link" icon={<EditOutlined />} onClick={() => openModal(record)} />
          <Popconfirm
            title="确定要删除这个原料出库吗？"
            onConfirm={() => deleteRawMaterialOutbound(record.Id)}
            okText="确定"
            cancelText="取消"
          >
            <Button type="link" danger icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      )
    }
  ]

  return (
    <div>
      <Card
        title="原材料出库管理"
        extra={
          <Button type="primary" icon={<PlusOutlined />} onClick={() => openModal()}>
            新增原材料出库
          </Button>
        }
      >
        <Table
          columns={columns}
          dataSource={dataSource}
          rowKey="Id"
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
          scroll={{ x: 1200 }}
        />
      </Card>

      {/* 添加/编辑原料出库模态框 */}
      <Modal
        title={editingRecord ? '编辑原材料出库单' : '新增原材料出库单'}
        open={modalVisible}
        onCancel={closeModal}
        footer={null}
        width={800}
      >
        <Form form={form} layout="vertical">
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16 }}>
            <Form.Item
              name="OutboundNumber"
              label="*出库单号"
              rules={[{ required: true, message: '请输入出库单号' }]}
            >
              <Input placeholder="请输入出库单号" disabled />
            </Form.Item>

            <Form.Item
              name="OutboundDate"
              label="*出库日期"
              rules={[{ required: true, message: '请选择出库日期' }]}
            >
              <DatePicker style={{ width: '100%' }} />
            </Form.Item>
          </div>

          <div style={{ margin: '20px 0' }}>
            <h3>出库明细</h3>
            <div style={{ display: 'grid', gridTemplateColumns: '200px 120px 100px 100px 200px', gap: 16, marginBottom: 16, fontWeight: 'bold' }}>
              <div>原材料</div>
              <div>规格</div>
              <div>数量</div>
              <div>可用数量</div>
              <div>备注</div>
            </div>
            <Form.List name="Items">
              {(fields, { add, remove }) => (
                <>
                  {fields.map((field, index) => {
                    const { key, ...fieldProps } = field;
                    return (
                      <div key={key} style={{ display: 'flex', gap: 16, marginBottom: 16, alignItems: 'center' }}>
                        <Form.Item
                          {...fieldProps}
                          name={[field.name, 'RawMaterialId']}
                          fieldKey={[field.fieldKey, 'RawMaterialId']}
                          rules={[{ required: true, message: '请选择原材料' }]}
                          style={{ width: '200px', marginBottom: 0 }}
                        >
                          <Select 
                            placeholder="请输入或选择原材料" 
                            showSearch
                            style={{ width: '100%' }}
                            styles={{
                              popup: {
                                root: {
                                  maxHeight: 300,
                                  overflowY: 'auto'
                                }
                              }
                            }}
                            optionLabelProp="label"
                            options={rawMaterials.map(rawMaterial => ({
                              label: rawMaterial.productName,
                              value: String(rawMaterial.id)
                            }))}
                            labelRender={(label) => {
                              if (typeof label === 'string') {
                                return (
                                  <div style={{ 
                                    whiteSpace: 'nowrap', 
                                    overflow: 'hidden', 
                                    textOverflow: 'ellipsis', 
                                    maxWidth: '100%' 
                                  }}>
                                    {label}
                                  </div>
                                );
                              }
                              if (typeof label === 'object' && label !== null) {
                                return String(label.label || '');
                              }
                              return '';
                            }}
                            filterOption={(input, option) => {
                              if (option && option.label) {
                                const label = typeof option.label === 'string' ? option.label : '';
                                return label.toLowerCase().indexOf(input.toLowerCase()) >= 0;
                              }
                              return false;
                            }}
                            onSearch={(value) => {
                              // 当用户输入时，我们可以在这里处理搜索逻辑
                            }}
                            onChange={(value) => {
                              // 处理选择逻辑
                              const selectedValue = value;
                              const selectedMaterial = rawMaterials.find(m => String(m.id) === selectedValue);
                              if (selectedMaterial || selectedValue) {
                                // 获取当前的Items数据
                                const currentItems = form.getFieldValue('Items') || [];
                                // 创建新的Items数组，只更新当前索引的数据
                                const newItems = [...currentItems];
                                newItems[index] = {
                                  ...newItems[index],
                                  RawMaterialId: selectedValue,
                                  Specification: selectedMaterial ? selectedMaterial.specification : '',
                                  Quantity: 1
                                };
                                // 更新Items字段
                                form.setFieldsValue({ 'Items': newItems });
                              }
                            }}
                          />
                        </Form.Item>
                        <Form.Item
                          {...fieldProps}
                          name={[field.name, 'Specification']}
                          fieldKey={[field.fieldKey, 'Specification']}
                          style={{ width: '120px', marginBottom: 0 }}
                        >
                          <Input placeholder="规格" />
                        </Form.Item>
                        <Form.Item
                          {...fieldProps}
                          name={[field.name, 'Quantity']}
                          fieldKey={[field.fieldKey, 'Quantity']}
                          initialValue={1}
                          style={{ width: '100px', marginBottom: 0 }}
                        >
                          <Input type="number" min={1} placeholder="1" />
                        </Form.Item>
                        <Form.Item
                          fieldKey={[field.fieldKey, 'availableQuantity']}
                          style={{ width: '100px', marginBottom: 0 }}
                        >
                          <Input 
                            value={(function() {
                              const rawMaterialId = form.getFieldValue(['Items', field.name, 'RawMaterialId']);
                              const selectedMaterial = rawMaterialMap[rawMaterialId];
                              return `已有: ${selectedMaterial ? selectedMaterial.remainingQuantity || 0 : 0}`;
                            })()}
                            disabled 
                          />
                        </Form.Item>
                        <Form.Item
                          {...fieldProps}
                          name={[field.name, 'Remark']}
                          fieldKey={[field.fieldKey, 'Remark']}
                          style={{ flex: 1, marginBottom: 0 }}
                        >
                          <Input placeholder="备注" />
                        </Form.Item>
                        <Button danger shape="circle" icon={<DeleteOutlined />} onClick={() => remove(index)} />
                      </div>
                    );
                  })}
                  <Button type="dashed" onClick={() => add({})} style={{ width: '100%', marginTop: 8 }}>
                    ⊕ 添加明细行
                  </Button>
                </>
              )}
            </Form.List>
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16 }}>
            <Form.Item
              name="Recipient"
              label="收货人"
              rules={[{ required: true, message: '请输入收货人' }]}
            >
              <Input placeholder="请输入收货人" />
            </Form.Item>

            <Form.Item
              name="Operator"
              label="操作人"
              rules={[{ required: true, message: '请输入操作人' }]}
            >
              <Input placeholder="请输入操作人" />
            </Form.Item>
          </div>

          <Form.Item
            name="Remark"
            label="出库单备注"
          >
            <Input.TextArea rows={4} placeholder="请输入出库单备注 (可选)" />
          </Form.Item>

          <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 16, marginTop: 20 }}>
            <Button icon={<EyeOutlined />} onClick={() => openPreviewModal(form.getFieldsValue())}>预览</Button>
            <Button onClick={closeModal}>取消</Button>
            <Button type="primary" onClick={saveRawMaterialOutbound}>确定</Button>
          </div>
        </Form>
      </Modal>

      {/* 预览模态框 */}
      <Modal
        title="原材料出库单预览"
        open={previewModalVisible}
        onCancel={closePreviewModal}
        footer={[
          <Button key="print" icon={<FileTextOutlined />}>打印</Button>,
          <Button key="export" icon={<SaveOutlined />}>保存PDF</Button>,
          <Button key="close" type="primary" onClick={closePreviewModal}>关闭预览</Button>
        ]}
        width={600}
      >
        {previewRecord && (
          <div style={{ padding: 20 }}>
            <h2 style={{ textAlign: 'center', marginBottom: 30 }}>原材料出库单</h2>
            <div style={{ marginBottom: 20, display: 'grid', gridTemplateColumns: '1fr 1fr 1fr' }}>
              <div>
                <strong>出库单号：</strong>{previewRecord.OutboundNumber}
              </div>
              <div>
                <strong>出库日期：</strong>{previewRecord.OutboundDate ? previewRecord.OutboundDate.format('YYYY-MM-DD') : '-'}
              </div>
              <div>
                <strong>收货人：</strong>{previewRecord.Recipient || '-'}
              </div>
            </div>
            <div style={{ marginBottom: 10 }}>
              <strong>操作人：</strong>{previewRecord.Operator || '-'}
            </div>
            <div style={{ marginBottom: 20 }}>
              <h3>出库明细</h3>
              <table style={{ width: '100%', 'border-collapse': 'collapse', marginTop: 10 }}>
                <thead>
                  <tr style={{ 'border-bottom': '1px solid #ddd' }}>
                    <th style={{ padding: '8px', textAlign: 'left' }}>序号</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>原材料</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>规格</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>数量</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>备注</th>
                  </tr>
                </thead>
                <tbody>
                  {previewRecord.Items && previewRecord.Items.map((item, index) => (
                    <tr key={index} style={{ borderBottom: '1px solid #f0f0f0' }}>
                      <td style={{ padding: '8px' }}>{index + 1}</td>
                      <td style={{ padding: '8px' }}>{rawMaterialMap[item.RawMaterialId]?.productName || '未知'}</td>
                      <td style={{ padding: '8px' }}>{item.Specification || '-'}</td>
                      <td style={{ padding: '8px' }}>{item.Quantity || 0}</td>
                      <td style={{ padding: '8px' }}>{item.Remark || '-'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            <div style={{ marginBottom: 10 }}>
              <strong>出库单备注</strong>
              <div style={{ marginTop: 8, padding: 12, border: '1px solid #f0f0f0', borderRadius: 4, minHeight: 80 }}>
                {previewRecord.Remark || '无'}
              </div>
            </div>
          </div>
        )}
      </Modal>
    </div>
  )
}

export default RawMaterialOutbound