import React, { useState, useEffect, useCallback } from 'react';
import { Card, Table, Button, Input, Select, Space, message, Modal, Form, Upload, Image, Popconfirm, Tag, InputNumber, Row, Col } from 'antd';
import { SearchOutlined, EditOutlined, DeleteOutlined, PlusOutlined, ImportOutlined, DownloadOutlined, DeleteTwoTone } from '@ant-design/icons';
import usePermission from '../hooks/usePermission';
import consumableApi from '../api/consumable';
import { debounce } from '../utils/cache';
import './ConsumableManagement.css';

const { Option } = Select;
const { Search } = Input;

const ConsumableManagement = () => {
  const [consumables, setConsumables] = useState([]);
  const [loading, setLoading] = useState(false);
  const [searchKeyword, setSearchKeyword] = useState('');
  const [modalVisible, setModalVisible] = useState(false);
  const [importModalVisible, setImportModalVisible] = useState(false);
  const [editingConsumable, setEditingConsumable] = useState(null);
  const [form] = Form.useForm();
  const [imageUrl, setImageUrl] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [total, setTotal] = useState(0);
  const { canAction } = usePermission();

  // 获取耗材列表
  const fetchConsumables = async (page = 1, size = 10, keyword = '') => {
    setLoading(true);
    try {
      const data = await consumableApi.getConsumables({
        pageNumber: page,
        pageSize: size,
        keyword: keyword
      });
      if (data && data.success) {
        setConsumables(data.data.items || []);
        setTotal(data.data.totalCount || data.data.total || 0);
      }
    } catch (error) {
      message.error('获取耗材列表失败');
      console.error('Error fetching consumables:', error);
    } finally {
      setLoading(false);
    }
  };

  // 初始加载
  useEffect(() => {
    fetchConsumables(currentPage, pageSize, searchKeyword);
  }, [currentPage, pageSize, searchKeyword]);

  // 使用防抖优化搜索
  const handleSearch = useCallback(debounce((value) => {
    setSearchKeyword(value);
    fetchConsumables(1, pageSize, value);
  }, 300), [pageSize]);

  // 处理添加耗材
  const handleAddConsumable = () => {
    setEditingConsumable(null);
    form.resetFields();
    setImageUrl('');
    setModalVisible(true);
  };

  // 处理编辑耗材
  const handleEditConsumable = (consumable) => {
    setEditingConsumable(consumable);
    form.setFieldsValue(consumable);
    setImageUrl(consumable.image || '');
    setModalVisible(true);
  };

  // 处理清空数量
  const handleClearQuantity = async (id) => {
    try {
      await consumableApi.updateConsumable(id, { remainingQuantity: 0, usedQuantity: 0 });
      setConsumables(consumables.map(item => 
        item.id === id ? { ...item, remainingQuantity: 0, usedQuantity: 0 } : item
      ));
      message.success('数量已清空');
    } catch (error) {
      message.error('清空数量失败');
      console.error('Error clearing quantity:', error);
    }
  };

  // 处理表单提交
  const handleSubmit = async (values) => {
    try {
      if (editingConsumable) {
        // 更新耗材
        await consumableApi.updateConsumable(editingConsumable.id, values);
        setConsumables(consumables.map(item => 
          item.id === editingConsumable.id ? { ...item, ...values } : item
        ));
        message.success('耗材更新成功');
        setModalVisible(false);
      } else {
        // 创建耗材
        const responseData = await consumableApi.createConsumable(values);
        const newConsumable = responseData.data;
        setConsumables([...consumables, newConsumable]);
        message.success('耗材创建成功');
        setModalVisible(false);
      }
    } catch (error) {
      message.error('操作失败');
      console.error('Error submitting form:', error);
    }
  };

  // 处理图片上传
  const handleImageUpload = (info) => {
    if (info.file.status === 'done') {
      // 假设上传成功后返回图片URL
      setImageUrl(info.file.response.url);
      form.setFieldsValue({ image: info.file.response.url });
    } else if (info.file.status === 'error') {
      message.error('图片上传失败');
    }
  };

  // 处理导入Excel
  const handleImportExcel = () => {
    setImportModalVisible(true);
  };

  // 处理开始导入
  const handleStartImport = () => {
    message.success('Excel导入功能开发中');
    setImportModalVisible(false);
  };

  // 处理下载模板
  const handleDownloadTemplate = () => {
    message.success('模板下载功能开发中');
  };

  // 处理清空库存
  const handleClearInventory = async () => {
    try {
      await consumableApi.deleteAllConsumables();
      setConsumables([]);
      message.success('库存已清空');
    } catch (error) {
      message.error('清空库存失败');
      console.error('Error clearing inventory:', error);
    }
  };

  // 表格列定义
  const columns = [
    {
      title: '序号',
      dataIndex: 'id',
      key: 'id',
      width: 60,
      render: (_, record, index) => (currentPage - 1) * pageSize + index + 1
    },
    {
      title: '名称',
      dataIndex: 'name',
      key: 'name',
      width: 150
    },
    {
      title: '品牌',
      dataIndex: 'brand',
      key: 'brand',
      width: 100
    },
    {
      title: '型号规格',
      dataIndex: 'modelSpecification',
      key: 'modelSpecification',
      width: 120
    },
    {
      title: '总数',
      dataIndex: 'totalQuantity',
      key: 'totalQuantity',
      width: 80
    },
    {
      title: '已使用数',
      dataIndex: 'usedQuantity',
      key: 'usedQuantity',
      width: 100,
      render: (num) => {
        return <Tag color="blue">{num || 0}</Tag>;
      }
    },
    {
      title: '剩余数量',
      dataIndex: 'remainingQuantity',
      key: 'remainingQuantity',
      width: 100,
      render: (num) => {
        let color = 'green';
        if (num === 0) color = 'red';
        else if (num < 10) color = 'orange';
        return <Tag color={color}>{num}</Tag>;
      }
    },
    {
      title: '单位',
      dataIndex: 'unit',
      key: 'unit',
      width: 80
    },
    {
      title: '所在仓库',
      dataIndex: 'location',
      key: 'location',
      width: 120
    },
    {
      title: '所属公司',
      dataIndex: 'company',
      key: 'company',
      width: 150
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
      width: 150
    },
    {
      title: '操作',
      key: 'action',
      width: 150,
      render: (_, record) => {
        return (
          <Space size="middle">
            <Button type="primary" icon={<EditOutlined />} onClick={() => handleEditConsumable(record)} />
            <Button danger icon={<DeleteOutlined />} onClick={() => handleClearQuantity(record.id)} />
          </Space>
        );
      }
    }
  ];

  return (
    <div className="consumable-management">
      <Card
        title="耗材管理"
        extra={
          <Space>
            <Search
              placeholder="搜索耗材"
              value={searchKeyword}
              onChange={(e) => setSearchKeyword(e.target.value)}
              onSearch={handleSearch}
              style={{ width: 200 }}
              allowClear
            />
            <Button icon={<ImportOutlined />} onClick={handleImportExcel}>导入Excel</Button>
            {canAction('create') && (
              <Button type="primary" icon={<PlusOutlined />} onClick={handleAddConsumable}>新增耗材</Button>
            )}
            <Button danger icon={<DeleteTwoTone />} onClick={handleClearInventory}>清空库存</Button>
          </Space>
        }
      >
        <Table
          columns={columns}
          dataSource={consumables}
          rowKey={(record) => record.id || Math.random().toString(36)}
          loading={loading}
          pagination={{
            current: currentPage,
            pageSize: pageSize,
            total: total,
            onChange: (page, size) => {
              setCurrentPage(page);
              setPageSize(size);
            },
            showSizeChanger: true,
            showQuickJumper: true,
            showTotal: (total) => `共 ${total} 条`
          }}
          scroll={{ x: 1600 }}
        />
      </Card>

      {/* 导入Excel模态框 */}
      <Modal
        title="导入Excel文件"
        open={importModalVisible}
        onCancel={() => setImportModalVisible(false)}
        footer={[
          <Button key="cancel" onClick={() => setImportModalVisible(false)}>取消</Button>,
          <Button key="ok" type="primary" onClick={handleStartImport}>开始导入</Button>
        ]}
        width={500}
      >
        <Space direction="vertical" style={{ width: '100%' }}>
          <Space>
            <Button type="primary" icon={<ImportOutlined />}>导入Excel</Button>
            <Select defaultValue="template">
              <Option value="template">下载模板</Option>
            </Select>
          </Space>
          <div style={{ fontSize: '12px', color: '#666' }}>
            <p>请上传包含耗材信息的Excel文件，支持.xls和.xlsx格式</p>
            <p style={{ marginTop: 8 }}><strong>表格格式要求：</strong></p>
            <ul style={{ marginTop: 4, paddingLeft: 20 }}>
              <li>必须包含：耗材名称、剩余数量</li>
              <li>可选列：品牌、型号规格、单位、所在仓库、所属公司、配件、备注、图片</li>
            </ul>
          </div>
        </Space>
      </Modal>

      {/* 添加/编辑耗材模态框 */}
      <Modal
        title={editingConsumable ? '编辑耗材' : '新增耗材'}
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
          <Row gutter={16}>
            <Col span={6}>
              <Form.Item
                name="name"
                label="名称"
                rules={[{ required: true, message: '请输入名称' }]}
              >
                <Input placeholder="请输入名称" />
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item
                name="brand"
                label="品牌"
              >
                <Input placeholder="请输入品牌" />
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item
                name="modelSpecification"
                label="型号规格"
              >
                <Input placeholder="请输入型号规格" />
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item
                name="remainingQuantity"
                label="剩余数量"
                rules={[{ required: true, message: '请输入剩余数量' }]}
              >
                <InputNumber min={0} style={{ width: '100%' }} placeholder="0" />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={16}>
            <Col span={6}>
              <Form.Item
                name="unit"
                label="单位"
              >
                <Input placeholder="如: 个、包、卷" />
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item
                name="location"
                label="所在仓库"
              >
                <Input placeholder="请输入所在仓库" />
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item
                name="company"
                label="所属公司"
              >
                <Input placeholder="请输入所属公司" />
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item
                name="accessories"
                label="配件"
              >
                <Input placeholder="请输入配件信息" />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="remark"
                label="备注"
              >
                <Input.TextArea placeholder="请输入备注" rows={1} />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="image"
                label="图片"
              >
                <Upload
                  name="file"
                  action="/api/upload"
                  listType="picture"
                  fileList={form.getFieldValue('image') ? [{ url: form.getFieldValue('image') }] : []}
                >
                  <Button icon={<PlusOutlined />}>上传图片</Button>
                </Upload>
              </Form.Item>
            </Col>
          </Row>

          <Form.Item>
            <Space style={{ float: 'right' }}>
              <Button onClick={() => setModalVisible(false)}>取消</Button>
              <Button type="primary" htmlType="submit">确定</Button>
            </Space>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default ConsumableManagement