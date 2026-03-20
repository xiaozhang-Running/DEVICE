import React, { useState, useEffect, useCallback } from 'react';
import { Card, Table, Button, Input, Select, Space, message, Modal, Form, Upload, Image, Popconfirm, Tag, InputNumber, Row, Col } from 'antd';
import { SearchOutlined, EditOutlined, DeleteOutlined, PlusOutlined, FilterOutlined, ImportOutlined, ReloadOutlined, DeleteTwoTone } from '@ant-design/icons';
import usePermission from '../hooks/usePermission';
import generalEquipmentApi from '../api/generalEquipment';
import { debounce } from '../utils/cache';
import './GeneralEquipmentManagement.css';

const { Option } = Select;
const { Search } = Input;

const GeneralEquipmentManagement = () => {
  const [equipments, setEquipments] = useState([]);
  const [loading, setLoading] = useState(false);
  const [searchKeyword, setSearchKeyword] = useState('');
  const [filters, setFilters] = useState({
    deviceStatus: null,
    useStatus: null,
    brand: ''
  });
  const [modalVisible, setModalVisible] = useState(false);
  const [importModalVisible, setImportModalVisible] = useState(false);
  const [editingEquipment, setEditingEquipment] = useState(null);
  const [form] = Form.useForm();
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [total, setTotal] = useState(0);
  const [isMounted, setIsMounted] = useState(true);
  const { canAction } = usePermission();

  // 组件卸载时设置isMounted为false
  useEffect(() => {
    return () => {
      setIsMounted(false);
    };
  }, []);

  const fetchEquipments = async (page = 1, size = 20, keyword = '', filterParams = {}) => {
    if (!isMounted) return; // 组件已卸载，直接返回
    setLoading(true);
    try {
      const params = {
        pageNumber: page,
        pageSize: size,
        keyword: keyword || undefined,
        deviceStatus: filterParams.deviceStatus || undefined,
        useStatus: filterParams.useStatus || undefined,
        brand: filterParams.brand || undefined
      };
      const response = await generalEquipmentApi.getAll(params);
      if (!isMounted) return; // 组件已卸载，直接返回
      if (response.success && response.data) {
        setEquipments(response.data.items || []);
        setTotal(response.data.totalCount || response.data.total || 0);
      }
    } catch (error) {
      if (!isMounted) return; // 组件已卸载，直接返回
      message.error('获取通用设备列表失败');
      console.error('Error fetching general equipments:', error);
    } finally {
      if (isMounted) {
        setLoading(false);
      }
    }
  };

  useEffect(() => {
    fetchEquipments(currentPage, pageSize, searchKeyword, filters);
  }, [currentPage, pageSize, searchKeyword, filters]);

  const handleSearch = useCallback(debounce((value) => {
    setSearchKeyword(value);
    setCurrentPage(1);
  }, 300), []);

  const handleFilter = () => {
    setCurrentPage(1);
  };

  const handleResetFilters = () => {
    setFilters({
      deviceStatus: null,
      useStatus: null,
      brand: ''
    });
    setSearchKeyword('');
    setCurrentPage(1);
  };

  const handleAddEquipment = () => {
    setEditingEquipment(null);
    form.resetFields();
    form.setFieldsValue({
      deviceType: 2,
      deviceStatus: 1,
      useStatus: 0,
      quantity: 1
    });
    setModalVisible(true);
  };

  const handleEditEquipment = (equipment) => {
    setEditingEquipment(equipment);
    form.setFieldsValue({
      ...equipment,
      deviceType: equipment.deviceType || 2,
      deviceStatus: equipment.deviceStatus || 1,
      useStatus: equipment.useStatus || 0
    });
    setModalVisible(true);
  };

  const handleDeleteEquipment = async (id) => {
    if (!isMounted) return; // 组件已卸载，直接返回
    try {
      await generalEquipmentApi.delete(id);
      if (!isMounted) return; // 组件已卸载，直接返回
      setEquipments(equipments.filter(item => item.id !== id));
      setTotal(total - 1);
      message.success('通用设备删除成功');
    } catch (error) {
      if (!isMounted) return; // 组件已卸载，直接返回
      message.error('通用设备删除失败');
      console.error('Error deleting general equipment:', error);
    }
  };

  const handleSubmit = async (values) => {
    if (!isMounted) return; // 组件已卸载，直接返回
    try {
      if (editingEquipment) {
        await generalEquipmentApi.update(editingEquipment.id, values);
        if (!isMounted) return; // 组件已卸载，直接返回
        setEquipments(equipments.map(item => 
          item.id === editingEquipment.id ? { ...item, ...values } : item
        ));
        message.success('通用设备更新成功');
        setModalVisible(false);
      } else {
        const response = await generalEquipmentApi.create(values);
        if (!isMounted) return; // 组件已卸载，直接返回
        const newEquipment = response.data;
        setEquipments([newEquipment, ...equipments]);
        setTotal(total + 1);
        message.success('通用设备创建成功');
        setModalVisible(false);
      }
    } catch (error) {
      if (!isMounted) return; // 组件已卸载，直接返回
      message.error('操作失败');
      console.error('Error submitting form:', error);
    }
  };

  const getDeviceStatusText = (status) => {
    switch (status) {
      case 1: return '正常';
      case 2: return '损坏';
      case 3: return '报废';
      default: return '未知';
    }
  };

  const getDeviceStatusColor = (status) => {
    switch (status) {
      case 1: return 'green';
      case 2: return 'orange';
      case 3: return 'red';
      default: return 'default';
    }
  };

  const getUseStatusText = (status) => {
    switch (status) {
      case 0: return '未使用';
      case 1: return '使用中';
      default: return '未知';
    }
  };

  const getUseStatusColor = (status) => {
    switch (status) {
      case 0: return 'blue';
      case 1: return 'orange';
      default: return 'default';
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
      title: '设备名称',
      dataIndex: 'deviceName',
      key: 'deviceName',
      width: 180
    },
    {
      title: '设备编号',
      dataIndex: 'deviceCode',
      key: 'deviceCode',
      width: 150
    },
    {
      title: 'SN码',
      dataIndex: 'serialNumber',
      key: 'serialNumber',
      width: 150
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
      width: 120
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
      width: 120
    },
    {
      title: '备注',
      dataIndex: 'remark',
      key: 'remark',
      width: 120
    },
    {
      title: '图片',
      dataIndex: 'imageUrl',
      key: 'imageUrl',
      width: 80,
      render: (url) => url ? <Image width={50} src={url} /> : '-'
    },
    {
      title: '设备状态',
      dataIndex: 'deviceStatus',
      key: 'deviceStatus',
      width: 100,
      render: (status) => (
        <Tag color={getDeviceStatusColor(status)}>{getDeviceStatusText(status)}</Tag>
      )
    },
    {
      title: '使用状态',
      dataIndex: 'useStatus',
      key: 'useStatus',
      width: 100,
      render: (status) => (
        <Tag color={getUseStatusColor(status)}>{getUseStatusText(status)}</Tag>
      )
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
      width: 140,
      fixed: 'right',
      render: (_, record) => (
        <Space size="middle">
          {canAction('edit') && (
            <Button 
              type="primary" 
              icon={<EditOutlined />} 
              onClick={() => handleEditEquipment(record)}
            />
          )}
          {canAction('delete') && (
            <Popconfirm
              title="确定要删除这个通用设备吗？"
              onConfirm={() => handleDeleteEquipment(record.id)}
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
    <div className="general-equipment-management">
      <Card
        title="通用设备管理"
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
              placeholder="设备状态"
              value={filters.deviceStatus}
              onChange={(value) => setFilters({ ...filters, deviceStatus: value })}
              style={{ width: 120 }}
              allowClear
            >
              <Option value={1}>正常</Option>
              <Option value={2}>损坏</Option>
              <Option value={3}>报废</Option>
            </Select>
            <Select
              placeholder="使用状态"
              value={filters.useStatus}
              onChange={(value) => setFilters({ ...filters, useStatus: value })}
              style={{ width: 120 }}
              allowClear
            >
              <Option value={0}>未使用</Option>
              <Option value={1}>使用中</Option>
            </Select>
            <Input
              placeholder="品牌"
              value={filters.brand}
              onChange={(e) => setFilters({ ...filters, brand: e.target.value })}
              style={{ width: 120 }}
              allowClear
            />
            <Button type="primary" onClick={handleFilter}>筛选</Button>
            <Button icon={<ImportOutlined />} onClick={() => setImportModalVisible(true)}>
              导入Excel
            </Button>
            {canAction('create') && (
              <Button type="primary" icon={<PlusOutlined />} onClick={handleAddEquipment}>
                新增设备
              </Button>
            )}
            <Button danger icon={<DeleteTwoTone />} onClick={handleResetFilters}>
              清空库存
            </Button>
          </Space>
        }
      >
        <Table
          columns={columns}
          dataSource={equipments}
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
          scroll={{ x: 2400 }}
        />
      </Card>

      <Modal
        title={editingEquipment ? '编辑设备' : '新增设备'}
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
                name="deviceType"
                label="设备类型"
                rules={[{ required: true, message: '请选择设备类型' }]}
              >
                <Select>
                  <Option value={1}>专用设备</Option>
                  <Option value={2}>通用设备</Option>
                  <Option value={3}>耗材</Option>
                  <Option value={4}>原材料</Option>
                </Select>
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item
                name="deviceName"
                label="设备名称"
                rules={[{ required: true, message: '请输入设备名称' }]}
              >
                <Input placeholder="请输入设备名称" />
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item
                name="deviceCode"
                label="设备编号"
                rules={[{ required: true, message: '请输入设备编号' }]}
              >
                <Input placeholder="请输入设备编号" />
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item
                name="serialNumber"
                label="SN码"
              >
                <Input placeholder="请输入SN码" />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={16}>
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
                name="model"
                label="型号"
              >
                <Input placeholder="请输入型号" />
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item
                name="quantity"
                label="数量"
                rules={[{ required: true, message: '请输入数量' }]}
              >
                <InputNumber min={1} style={{ width: '100%' }} placeholder="1" />
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item
                name="unit"
                label="单位"
              >
                <Input placeholder="如：台、个" />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={16}>
            <Col span={6}>
              <Form.Item
                name="imageUrl"
                label="图片"
              >
                <Upload
                  name="file"
                  action="/api/upload"
                  listType="picture"
                  fileList={form.getFieldValue('imageUrl') ? [{ url: form.getFieldValue('imageUrl') }] : []}
                >
                  <Button icon={<PlusOutlined />}>上传图片</Button>
                </Upload>
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item
                name="deviceStatus"
                label="设备状态"
                rules={[{ required: true, message: '请选择设备状态' }]}
              >
                <Select>
                  <Option value={1}>正常</Option>
                  <Option value={2}>损坏</Option>
                  <Option value={3}>报废</Option>
                </Select>
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item
                name="useStatus"
                label="使用状态"
                rules={[{ required: true, message: '请选择使用状态' }]}
              >
                <Select>
                  <Option value={0}>未使用</Option>
                  <Option value={1}>使用中</Option>
                </Select>
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
          </Row>

          <Row gutter={16}>
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
                name="accessories"
                label="配件"
              >
                <Input placeholder="请输入配件信息" />
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
          <Button key="ok" type="primary">开始导入</Button>
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
            <p>请上传包含通用设备信息的Excel文件，支持.xls和.xlsx格式</p>
            <p style={{ marginTop: 8 }}><strong>表格格式要求：</strong></p>
            <ul style={{ marginTop: 4, paddingLeft: 20 }}>
              <li>必须包含：设备名称、设备编号</li>
              <li>可选列：SN码、品牌、型号、数量、单位、配件、备注、所属公司、所在仓库、设备状态、使用状态</li>
              <li>设备状态：正常/损坏/报废</li>
              <li>使用状态：未使用/使用中</li>
            </ul>
          </div>
        </Space>
      </Modal>
    </div>
  );
};

export default GeneralEquipmentManagement