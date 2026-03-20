import React, { useState, useEffect } from 'react'
import { Table, Button, Modal, Form, Input, Select, DatePicker, message, Space, Popconfirm, Tag, Card, AutoComplete } from 'antd'
import { PlusOutlined, EditOutlined, DeleteOutlined, EyeOutlined, FileTextOutlined, SaveOutlined, CheckOutlined } from '@ant-design/icons'
import rawMaterialInboundApi from '../api/rawMaterialInbound'
import rawMaterialApi from '../api/rawMaterial'
import dayjs from 'dayjs'

const { Option } = Select
const { TextArea } = Input

function RawMaterialInbound() {
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
  const [autoCompleteValues, setAutoCompleteValues] = useState({})

  // 加载原料列表
  const loadRawMaterials = async () => {
    try {
      const response = await rawMaterialApi.getRawMaterials()
      if (response.success && response.data) {
        const materials = response.data || []
        setRawMaterials(materials)
        // 创建原料ID到原料对象的映射，使用字符串作为键
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

  // 加载原料入库数据
  const loadRawMaterialInbounds = async () => {
    setLoading(true)
    try {
      const response = await rawMaterialInboundApi.getRawMaterialInbounds()
      if (response.success && response.data) {
        // 确保dataSource是数组
        if (Array.isArray(response.data)) {
          setDataSource(response.data)
          setTotal(response.data.length)
        } else if (response.data.data && Array.isArray(response.data.data)) {
          setDataSource(response.data.data)
          setTotal(response.data.data.length)
        } else if (response.data.data && Array.isArray(response.data.data.items)) {
          setDataSource(response.data.data.items)
          setTotal(response.data.data.total || 0)
        } else {
          setDataSource([])
          setTotal(0)
        }
      } else {
        setDataSource([])
        setTotal(0)
      }
    } catch (error) {
      message.error('加载原料入库数据失败')
      console.error('Error loading raw material inbounds:', error)
      setDataSource([])
      setTotal(0)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadRawMaterials()
    loadRawMaterialInbounds()
  }, [])

  // 打开添加/编辑模态框
  const openModal = (record = null) => {
    setEditingRecord(record)
    
    // 重置AutoComplete值
    setAutoCompleteValues({})
    
    if (record) {
      form.setFieldsValue({
        ...record,
        InboundDate: record.inboundDate ? dayjs(record.inboundDate) : null,
        Items: record.items || []
      })
      
      // 为现有记录的每个items生成AutoComplete显示值
      if (record.items && record.items.length > 0) {
        const newAutoCompleteValues = {}
        record.items.forEach((item, index) => {
          if (item.rawMaterialId) {
            const selectedMaterial = rawMaterialMap[String(item.rawMaterialId)]
            if (selectedMaterial) {
              newAutoCompleteValues[`${index}`] = selectedMaterial.productName
            } else {
              newAutoCompleteValues[`${index}`] = item.rawMaterialId
            }
          } else if (item.productName) {
            newAutoCompleteValues[`${index}`] = item.productName
          }
        })
        setAutoCompleteValues(newAutoCompleteValues)
      }
    } else {
      // 生成入库单号
      const today = new Date()
      const year = today.getFullYear()
      const month = String(today.getMonth() + 1).padStart(2, '0')
      const day = String(today.getDate()).padStart(2, '0')
      const random = Math.floor(Math.random() * 10000).toString().padStart(4, '0')
      const inboundNumber = `RK${year}${month}${day}${random}`
      
      form.resetFields()
      form.setFieldsValue({
        inboundNumber: inboundNumber,
        inboundDate: dayjs(),
        items: []
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

  // 保存原料入库
  const saveRawMaterialInbound = async () => {
    try {
      const values = await form.validateFields()
      
      // 转换日期格式
      const formattedValues = {
        ...values,
        inboundDate: values.inboundDate ? values.inboundDate.format('YYYY-MM-DD') : null,
        // 处理items字段，确保数据结构与后端API匹配
        items: values.items.map(item => {
          // 检查是否是新创建的原材料（RawMaterialId不是数字）
          if (typeof item.RawMaterialId === 'string' && isNaN(Number(item.RawMaterialId))) {
            return {
              productName: item.RawMaterialId,
              specification: item.Specification || '',
              quantity: item.Quantity || 1,
              remark: item.Remark || ''
            }
          }
          // 现有原材料
          return {
            rawMaterialId: typeof item.RawMaterialId === 'string' ? Number(item.RawMaterialId) : item.RawMaterialId,
            specification: item.Specification || '',
            quantity: item.Quantity || 1,
            remark: item.Remark || ''
          }
        })
      }
      
      if (editingRecord) {
        await rawMaterialInboundApi.updateRawMaterialInbound(editingRecord.id, formattedValues)
        message.success('更新原料入库成功')
      } else {
        await rawMaterialInboundApi.createRawMaterialInbound(formattedValues)
        message.success('创建原料入库成功')
      }
      
      closeModal()
      loadRawMaterialInbounds()
    } catch (error) {
      message.error('保存失败，请检查输入')
      console.error('Error saving raw material inbound:', error)
    }
  }

  // 删除原料入库
  const deleteRawMaterialInbound = async (id) => {
    try {
      await rawMaterialInboundApi.deleteRawMaterialInbound(id)
      message.success('删除原料入库成功')
      loadRawMaterialInbounds()
    } catch (error) {
      message.error('删除失败')
      console.error('Error deleting raw material inbound:', error)
    }
  }

  // 完成原料入库
  const completeRawMaterialInbound = async (id) => {
    try {
      await rawMaterialInboundApi.completeRawMaterialInbound(id)
      message.success('入库单完成成功')
      loadRawMaterialInbounds()
    } catch (error) {
      message.error('完成失败')
      console.error('Error completing raw material inbound:', error)
    }
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
      title: '入库明细',
      dataIndex: 'items',
      key: 'items',
      width: 200,
      render: (items) => {
        if (!items || items.length === 0) return '无明细';
        
        // 汇总显示入库的物品，最多显示3个，超过显示数量
        const displayItems = items.slice(0, 3);
        const itemTexts = displayItems.map(item => {
          // 尝试获取原材料名称
          let materialName = '未知材料';
          if (item.rawMaterialId) {
            const material = rawMaterialMap[item.rawMaterialId];
            if (material) {
              materialName = material.productName;
            }
          } else if (item.productName) {
            materialName = item.productName;
          }
          return `${materialName} (${item.quantity || 0})`;
        });
        
        if (items.length > 3) {
          itemTexts.push(`等${items.length - 3}项`);
        }
        
        return (
          <div style={{ lineHeight: '1.4' }}>
            {itemTexts.map((text, index) => (
              <div key={index}>{text}</div>
            ))}
          </div>
        );
      }
    },
    {
      title: '处理状态',
      dataIndex: 'status',
      key: 'status',
      width: 100,
      render: (status) => {
        let color = 'blue'
        let text = '待完成'
        if (status === 'completed') {
          color = 'green'
          text = '已完成'
        }
        return <Tag color={color}>{text}</Tag>
      }
    },
    {
      title: '交货人',
      dataIndex: 'deliveryPerson',
      key: 'deliveryPerson',
      width: 100
    },
    {
      title: '操作人',
      dataIndex: 'operator',
      key: 'operator',
      width: 100
    },

    {
      title: '备注',
      dataIndex: 'remark',
      key: 'remark',
      width: 150
    },
    {
      title: '操作',
      key: 'action',
      width: 150,
      render: (_, record) => (
        <Space size="middle">
          <Button type="link" icon={<EditOutlined />} onClick={() => openModal(record)} />
          {record.status !== 'completed' && (
            <Button type="link" icon={<CheckOutlined />} onClick={() => completeRawMaterialInbound(record.id)} />
          )}
          <Popconfirm
            title="确定要删除这个原料入库吗？"
            onConfirm={() => deleteRawMaterialInbound(record.id)}
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
        title="原材料采购入库管理"
        extra={
          <Button type="primary" icon={<PlusOutlined />} onClick={() => openModal()}>
            新增原材料入库
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
            }
          }}
          scroll={{ x: 1200 }}
        />
      </Card>

      {/* 添加/编辑原料入库模态框 */}
      <Modal
        title={editingRecord ? '编辑入库单' : '新增入库单'}
        open={modalVisible}
        onCancel={closeModal}
        footer={null}
        width={800}
      >
        <Form form={form} layout="vertical">
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16 }}>
            <Form.Item
              name="inboundNumber"
              label="*入库单号"
              rules={[{ required: true, message: '请输入入库单号' }]}
            >
              <Input placeholder="请输入入库单号" disabled />
            </Form.Item>

            <Form.Item
              name="inboundDate"
              label="*入库日期"
              rules={[{ required: true, message: '请选择入库日期' }]}
            >
              <DatePicker style={{ width: '100%' }} />
            </Form.Item>

            <Form.Item
              name="deliveryPerson"
              label="交货人"
              rules={[{ required: true, message: '请输入交货人' }]}
            >
              <Input placeholder="请输入交货人" />
            </Form.Item>

            <Form.Item
              name="operator"
              label="操作人"
              rules={[{ required: true, message: '请输入操作人' }]}
            >
              <Input placeholder="请输入操作人" />
            </Form.Item>
          </div>

          <div style={{ margin: '20px 0' }}>
            <h3>入库明细</h3>
            <div style={{ display: 'grid', gridTemplateColumns: '200px 120px 100px 100px 200px', gap: 16, marginBottom: 16, fontWeight: 'bold' }}>
              <div>原材料</div>
              <div>规格</div>
              <div>数量</div>
              <div>已有数量</div>
              <div>备注</div>
            </div>
            <Form.List name="items">
              {(fields, { add, remove }) => (
                <>
                  {fields.map((field, index) => {
                    const { key, ...fieldProps } = field;
                    return (
                      <div key={key} style={{ display: 'flex', gap: 16, marginBottom: 16, alignItems: 'center' }}>
                        <Form.Item
                          style={{ width: '200px', marginBottom: 0 }}
                        >
                          <AutoComplete
                            placeholder="请输入或选择原材料"
                            style={{ width: '100%' }}
                            options={rawMaterials.map(rawMaterial => ({
                              label: rawMaterial.productName,
                              value: rawMaterial.productName,
                              key: String(rawMaterial.id)
                            }))}
                            onChange={(value) => {
                              const selectedMaterial = rawMaterials.find(m => m.productName === value);
                              const currentItems = form.getFieldValue('items') || [];
                              const newItems = [...currentItems];
                              newItems[index] = {
                                ...newItems[index],
                                RawMaterialId: selectedMaterial ? String(selectedMaterial.id) : value,
                                Specification: selectedMaterial ? selectedMaterial.specification : '',
                                Quantity: 1
                              };
                              form.setFieldsValue({ 'items': newItems });
                            }}
                            onSelect={(value) => {
                              const selectedMaterial = rawMaterials.find(m => m.productName === value);
                              const currentItems = form.getFieldValue('items') || [];
                              const newItems = [...currentItems];
                              newItems[index] = {
                                ...newItems[index],
                                RawMaterialId: selectedMaterial ? String(selectedMaterial.id) : value,
                                Specification: selectedMaterial ? selectedMaterial.specification : '',
                                Quantity: 1
                              };
                              form.setFieldsValue({ 'items': newItems });
                            }}
                            onBlur={(e) => {
                              const currentValue = e.target.value;
                              const isExistingMaterial = rawMaterials.some(m => m.productName === currentValue);
                              if (currentValue && !isExistingMaterial) {
                                const currentItems = form.getFieldValue('items') || [];
                                const newItems = [...currentItems];
                                newItems[index] = {
                                  ...newItems[index],
                                  RawMaterialId: currentValue,
                                  Specification: newItems[index].Specification || '',
                                  Quantity: newItems[index].Quantity || 1
                                };
                                form.setFieldsValue({ 'items': newItems });
                              }
                            }}
                          >
                            <Input
                              placeholder="请输入或选择原材料"
                              value={(function() {
                                const rawMaterialId = form.getFieldValue(['items', field.name, 'RawMaterialId']);
                                if (rawMaterialId) {
                                  if (!isNaN(Number(rawMaterialId))) {
                                    const selectedMaterial = rawMaterialMap[String(rawMaterialId)];
                                    return selectedMaterial ? selectedMaterial.productName : rawMaterialId;
                                  } else {
                                    return rawMaterialId;
                                  }
                                }
                                return '';
                              })()}
                              onChange={(e) => {
                                // 当用户手动输入时，更新显示值
                                const value = e.target.value;
                                const currentItems = form.getFieldValue('items') || [];
                                const newItems = [...currentItems];
                                newItems[index] = {
                                  ...newItems[index],
                                  RawMaterialId: value
                                };
                                form.setFieldsValue({ 'items': newItems });
                              }}
                            />
                          </AutoComplete>
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
                          fieldKey={[field.fieldKey, 'existingQuantity']}
                          style={{ width: '100px', marginBottom: 0 }}
                        >
                          <Input
                            value={(function() {
                              const rawMaterialId = form.getFieldValue(['items', field.name, 'RawMaterialId']);
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
                          <Input placeholder="备注 (可选)" />
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

          <Form.Item
            name="remark"
            label="入库单备注"
          >
            <Input.TextArea rows={4} placeholder="请输入入库备注 (可选)" />
          </Form.Item>

          <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 16, marginTop: 20 }}>
            <Button icon={<EyeOutlined />} onClick={() => openPreviewModal(form.getFieldsValue())}>预览</Button>
            <Button onClick={closeModal}>取消</Button>
            <Button type="primary" onClick={saveRawMaterialInbound}>确定</Button>
          </div>
        </Form>
      </Modal>

      {/* 预览模态框 */}
      <Modal
        title="原材料采购入库单预览"
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
            <h2 style={{ textAlign: 'center', marginBottom: 30 }}>原材料采购入库单</h2>
            <div style={{ marginBottom: 20 }}>
              <div style={{ marginBottom: 10 }}>
                <strong>入库单号：</strong>{previewRecord.InboundNumber}
              </div>
            </div>
            <div style={{ marginBottom: 20 }}>
              <h3>入库明细</h3>
              <table style={{ width: '100%', 'border-collapse': 'collapse', marginTop: 10 }}>
                <thead>
                  <tr style={{ 'border-bottom': '1px solid #ddd' }}>
                    <th style={{ padding: '8px', textAlign: 'left' }}>序号</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>原材料名称</th>
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
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr 1fr', gap: 16, marginBottom: 10 }}>
              <div>
                <strong>交货人：</strong>{previewRecord.Handler || '-'}
              </div>
              <div>
                <strong>操作人：</strong>{previewRecord.Operator || '-'}
              </div>
              <div>
                <strong>状态：</strong>待完成
              </div>
              <div>
                <strong>入库日期：</strong>{previewRecord.InboundDate ? previewRecord.InboundDate.format('YYYY-MM-DD') : '-'}
              </div>
            </div>
            <div style={{ marginBottom: 10 }}>
              <strong>备注：</strong>{previewRecord.Remark || '-'}
            </div>
          </div>
        )}
      </Modal>
    </div>
  )
}

export default RawMaterialInbound