import React, { useState, useEffect, useCallback } from 'react';
import { Card, Table, Button, Input, Space, message, Modal, Form, Select, Popconfirm, Tag, Row, Col, InputNumber } from 'antd';
import { SearchOutlined, EditOutlined, DeleteOutlined, PlusOutlined, ImportOutlined, ReloadOutlined, DeleteTwoTone } from '@ant-design/icons';
import usePermission from '../hooks/usePermission';
import rawMaterialApi from '../api/rawMaterial';
import { debounce } from '../utils/cache';

const { Option } = Select;
const { Search } = Input;

const RawMaterialManagement = () => {
  const [rawMaterials, setRawMaterials] = useState([]);
  const [loading, setLoading] = useState(false);
  const [searchKeyword, setSearchKeyword] = useState('');
  const [modalVisible, setModalVisible] = useState(false);
  const [importModalVisible, setImportModalVisible] = useState(false);
  const [editingMaterial, setEditingMaterial] = useState(null);
  const [form] = Form.useForm();
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [total, setTotal] = useState(0);
  const { canAction } = usePermission();

  const fetchRawMaterials = async (keyword = '') => {
    setLoading(true);
    try {
      let response;
      if (keyword) {
        response = await rawMaterialApi.search(keyword);
      } else {
        response = await rawMaterialApi.getRawMaterials();
      }
      
      if (response.success && response.data) {
        const materials = Array.isArray(response.data) ? response.data : response.data.items || [];
        setRawMaterials(materials);
        setTotal(response.data.totalCount || response.data.total || materials.length);
      }
    } catch (error) {
      message.error('获取原材料列表失败');
      console.error('Error fetching raw materials:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchRawMaterials(searchKeyword);
  }, [searchKeyword]);

  const handleSearch = useCallback(debounce((value) => {
    setSearchKeyword(value);
  }, 300), []);

  const handleAddMaterial = () => {
    setEditingMaterial(null);
    form.resetFields();
    form.setFieldsValue({
      totalQuantity: 0,
      usedQuantity: 0,
      remainingQuantity: 0
    });
    setModalVisible(true);
  };

  const handleEditMaterial = (material) => {
    setEditingMaterial(material);
    form.setFieldsValue({
      ...material,
      remainingQuantity: material.remainingQuantity || 0
    });
    setModalVisible(true);
  };

  const handleDeleteMaterial = async (id) => {
    try {
      await rawMaterialApi.deleteRawMaterial(id);
      setRawMaterials(rawMaterials.filter(item => item.id !== id));
      setTotal(total - 1);
      message.success('原材料删除成功');
    } catch (error) {
      message.error('原材料删除失败');
      console.error('Error deleting raw material:', error);
    }
  };

  const handleClearQuantity = async (id) => {
    try {
      // 找到要更新的原材料
      const material = rawMaterials.find(item => item.id === id);
      if (!material) {
        message.error('原材料不存在');
        return;
      }
      
      // 包含所有必填字段
      await rawMaterialApi.updateRawMaterial(id, {
        productName: material.productName,
        totalQuantity: 0,
        usedQuantity: 0,
        remainingQuantity: 0
      });
      setRawMaterials(rawMaterials.map(item => 
        item.id === id ? { ...item, totalQuantity: 0, usedQuantity: 0, remainingQuantity: 0 } : item
      ));
      message.success('数量已清空');
    } catch (error) {
      message.error('清空数量失败');
      console.error('Error clearing quantity:', error);
    }
  };

  const handleSubmit = async (values) => {
    try {
      if (editingMaterial) {
        await rawMaterialApi.updateRawMaterial(editingMaterial.id, values);
        setRawMaterials(rawMaterials.map(item => 
          item.id === editingMaterial.id ? { ...item, ...values } : item
        ));
        message.success('原材料更新成功');
        setModalVisible(false);
      } else {
        const response = await rawMaterialApi.createRawMaterial(values);
        const newMaterial = response.data;
        setRawMaterials([newMaterial, ...rawMaterials]);
        setTotal(total + 1);
        message.success('原材料创建成功');
        setModalVisible(false);
      }
    } catch (error) {
      message.error('操作失败');
      console.error('Error submitting form:', error);
    }
  };

  const handleImportExcel = () => {
    message.info('导入Excel功能开发中');
  };

  const handleClearAll = async () => {
    try {
      await rawMaterialApi.deleteAll();
      setRawMaterials([]);
      setTotal(0);
      message.success('库存已清空');
    } catch (error) {
      message.error('清空库存失败');
      console.error('Error clearing all materials:', error);
    }
  };

  const columns = [
    {
      title: '序号',
      dataIndex: 'id',
      key: 'id',
      width: 60,
      render: (_, __, index) => (currentPage - 1) * pageSize + index + 1
    },
    {
      title: '产品名称',
      dataIndex: 'productName',
      key: 'productName',
      width: 180
    },
    {
      title: '规格',
      dataIndex: 'specification',
      key: 'specification',
      width: 150
    },
    {
      title: '总数',
      dataIndex: 'totalQuantity',
      key: 'totalQuantity',
      width: 80
    },
    {
      title: '已使用',
      dataIndex: 'usedQuantity',
      key: 'usedQuantity',
      width: 80
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
      title: '所属公司',
      dataIndex: 'company',
      key: 'company',
      width: 150
    },
    {
      title: '供应商',
      dataIndex: 'supplier',
      key: 'supplier',
      width: 150
    },
    {
      title: '备注',
      dataIndex: 'remark',
      key: 'remark',
      width: 200
    },
    {
      title: '操作',
      key: 'action',
      width: 150,
      fixed: 'right',
      render: (_, record) => (
        <Space size="middle">
          {canAction('edit') && (
            <Button 
              type="primary" 
              icon={<EditOutlined />} 
              onClick={() => handleEditMaterial(record)}
            />
          )}
          {canAction('delete') && (
            <Popconfirm
              title="确定要清空这个原材料的数量吗？"
              onConfirm={() => handleClearQuantity(record.id)}
              okText="确定"
              cancelText="取消"
            >
              <Button danger icon={<DeleteOutlined />} />
            </Popconfirm>
          )}
        </Space>
      )
    }
  ];

  return (
    <div className="raw-material-management">
      <Card
        title="原材料管理"
        extra={
          <Space>
            <Search
              placeholder="搜索产品名称/规格/供应商..."
              value={searchKeyword}
              onChange={(e) => setSearchKeyword(e.target.value)}
              onSearch={handleSearch}
              style={{ width: 300 }}
              allowClear
            />
            <Button icon={<ImportOutlined />} onClick={() => setImportModalVisible(true)}>
              导入Excel
            </Button>
            {canAction('create') && (
              <Button type="primary" icon={<PlusOutlined />} onClick={handleAddMaterial}>
                新增原材料
              </Button>
            )}
            <Button danger icon={<DeleteTwoTone />} onClick={handleClearAll}>
              清空库存
            </Button>
          </Space>
        }
      >
        <Table
          columns={columns}
          dataSource={rawMaterials}
          rowKey={(record) => record.Id || record.id || Math.random().toString(36)}
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

      <Modal
        title={editingMaterial ? '编辑原材料' : '新增原材料'}
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
                name="productName"
                label="*产品名称"
                rules={[{ required: true, message: '请输入产品名称' }]}
              >
                <Input placeholder="请输入产品名称" />
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item
                name="specification"
                label="规格"
              >
                <Input placeholder="请输入规格" />
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item
                name="remainingQuantity"
                label="*剩余数量"
                rules={[{ required: true, message: '请输入剩余数量' }]}
              >
                <InputNumber min={0} style={{ width: '100%' }} placeholder="0" />
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item
                name="unit"
                label="单位"
              >
                <Select placeholder="请选择单位">
                  <Option value="个">个</Option>
                  <Option value="块">块</Option>
                  <Option value="根">根</Option>
                  <Option value="张">张</Option>
                  <Option value="kg">kg</Option>
                  <Option value="L">L</Option>
                </Select>
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={16}>
            <Col span={6}>
              <Form.Item
                name="supplier"
                label="供应商"
              >
                <Input placeholder="请输入供应商" />
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
            <Col span={12}>
              <Form.Item
                name="remark"
                label="备注"
              >
                <Input.TextArea placeholder="请输入备注" rows={1} />
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

      <Modal
        title="导入Excel文件"
        open={importModalVisible}
        onCancel={() => setImportModalVisible(false)}
        footer={[
          <Button key="cancel" onClick={() => setImportModalVisible(false)}>取消</Button>,
          <Button key="ok" type="primary" onClick={handleImportExcel}>开始导入</Button>
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
            <p>请上传包含原材料信息的Excel文件，支持.xls和.xlsx格式</p>
            <p style={{ marginTop: 8 }}><strong>表格格式要求：</strong></p>
            <ul style={{ marginTop: 4, paddingLeft: 20 }}>
              <li>必须包含：产品名称、剩余数量</li>
              <li>可选列：规格、单位、供应商、所属公司、备注</li>
            </ul>
          </div>
        </Space>
      </Modal>
    </div>
  );
};

export default RawMaterialManagement