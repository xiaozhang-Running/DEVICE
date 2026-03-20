import React, { useState, useEffect, useCallback } from 'react';
import { Card, Table, Button, Input, Select, Space, message, Modal, Form, Upload, Image, Popconfirm, Tag, InputNumber, Row, Col } from 'antd';
import { SearchOutlined, EditOutlined, DeleteOutlined, PlusOutlined, ImportOutlined, FileExcelOutlined, DeleteTwoTone } from '@ant-design/icons';
import usePermission from '../hooks/usePermission';
import specialEquipmentApi from '../api/specialEquipment';
import { debounce } from '../utils/cache';

const { Option } = Select;
const { Search } = Input;
const { Dragger } = Upload;

const SpecialEquipmentManagement = () => {
  const [equipments, setEquipments] = useState([]);
  const [loading, setLoading] = useState(false);
  const [searchKeyword, setSearchKeyword] = useState('');
  const [deviceStatus, setDeviceStatus] = useState('');
  const [useStatus, setUseStatus] = useState('');
  const [brand, setBrand] = useState('');
  const [modalVisible, setModalVisible] = useState(false);
  const [importModalVisible, setImportModalVisible] = useState(false);
  const [editingEquipment, setEditingEquipment] = useState(null);
  const [form] = Form.useForm();
  const [imageUrl, setImageUrl] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [total, setTotal] = useState(0);
  const [isMounted, setIsMounted] = useState(true);
  const { canAction } = usePermission();

  // 组件卸载时设置isMounted为false
  useEffect(() => {
    return () => {
      setIsMounted(false);
    };
  }, []);

  // 获取专用设备列表
  const fetchEquipments = async (page = 1, size = 10, keyword = '', status = '', useStatusVal = '', brandVal = '') => {
    if (!isMounted) return; // 组件已卸载，直接返回
    setLoading(true);
    try {
      console.log('Fetching equipments with params:', { page, size, keyword, status, useStatusVal, brandVal });
      const data = await specialEquipmentApi.getAll({
        pageNumber: page,
        pageSize: size,
        keyword: keyword,
        deviceStatus: status,
        useStatus: useStatusVal,
        brand: brandVal
      });
      if (!isMounted) return; // 组件已卸载，直接返回
      console.log('API response:', data);
      console.log('API response data:', data.data);
      if (data) {
        // 检查响应格式
        if (data.items && Array.isArray(data.items)) {
          // 直接返回了分页数据
          console.log('Direct pagination data format');
          setEquipments(data.items);
          setTotal(data.total || 0);
        } else if (data.success && data.data) {
          // 标准响应格式
          console.log('Standard response format');
          if (data.data.items && Array.isArray(data.data.items)) {
            console.log('Pagination data in data field');
            setEquipments(data.data.items);
            setTotal(data.data.totalCount || data.data.total || 0);
          } else if (Array.isArray(data.data)) {
            console.log('Array data in data field');
            setEquipments(data.data);
            setTotal(data.data.length);
          } else {
            console.log('Unknown data format in data field');
            setEquipments([]);
            setTotal(0);
          }
        } else if (Array.isArray(data)) {
          // 直接返回了数组
          console.log('Direct array format');
          setEquipments(data);
          setTotal(data.length);
        } else {
          console.log('Unknown response format');
          setEquipments([]);
          setTotal(0);
        }
      } else {
        console.log('No response data');
        setEquipments([]);
        setTotal(0);
      }
    } catch (error) {
      if (!isMounted) return; // 组件已卸载，直接返回
      message.error('获取专用设备列表失败');
      console.error('Error fetching special equipments:', error);
      console.error('Error details:', error.response);
      setEquipments([]);
      setTotal(0);
    } finally {
      if (isMounted) {
        setLoading(false);
      }
    }
  };

  // 初始加载
  useEffect(() => {
    fetchEquipments(currentPage, pageSize, searchKeyword, deviceStatus, useStatus, brand);
  }, [currentPage, pageSize, searchKeyword, deviceStatus, useStatus, brand]);

  // 使用防抖优化搜索
  const handleSearch = useCallback(debounce((value) => {
    setSearchKeyword(value);
    setCurrentPage(1);
  }, 300), []);

  // 处理添加专用设备
  const handleAddEquipment = () => {
    setEditingEquipment(null);
    form.resetFields();
    setImageUrl('');
    setModalVisible(true);
  };

  // 处理编辑专用设备
  const handleEditEquipment = (equipment) => {
    setEditingEquipment(equipment);
    // 转换字段名以匹配后端DTO
    const formValues = {
      ...equipment,
      ImageUrl: equipment.image || equipment.imageUrl || equipment.Image || equipment.ImageUrl
    };
    form.setFieldsValue(formValues);
    setImageUrl(equipment.image || equipment.imageUrl || equipment.Image || equipment.ImageUrl || '');
    setModalVisible(true);
  };

  // 处理删除专用设备
  const handleDeleteEquipment = async (id) => {
    if (!isMounted) return; // 组件已卸载，直接返回
    try {
      await specialEquipmentApi.delete(id);
      if (!isMounted) return; // 组件已卸载，直接返回
      setEquipments(equipments.filter(item => item.id !== id));
      message.success('专用设备删除成功');
    } catch (error) {
      if (!isMounted) return; // 组件已卸载，直接返回
      message.error('专用设备删除失败');
      console.error('Error deleting special equipment:', error);
    }
  };

  // 处理表单提交
  const handleSubmit = async (values) => {
    if (!isMounted) return; // 组件已卸载，直接返回
    try {
      if (editingEquipment) {
        // 更新专用设备
        const equipmentId = editingEquipment.id;
        await specialEquipmentApi.update(equipmentId, values);
        if (!isMounted) return; // 组件已卸载，直接返回
        setEquipments(equipments.map(item => 
          item.id === equipmentId ? { ...item, ...values } : item
        ));
        message.success('专用设备更新成功');
        setModalVisible(false);
      } else {
        // 创建专用设备
        const responseData = await specialEquipmentApi.create(values);
        if (!isMounted) return; // 组件已卸载，直接返回
        const newEquipment = responseData.data;
        setEquipments([...equipments, newEquipment]);
        message.success('专用设备创建成功');
        setModalVisible(false);
      }
    } catch (error) {
      if (!isMounted) return; // 组件已卸载，直接返回
      message.error('操作失败');
      console.error('Error submitting form:', error);
    }
  };

  // 处理图片上传
  const handleImageUpload = (info) => {
    if (info.file.status === 'done') {
      // 假设上传成功后返回图片URL
      setImageUrl(info.file.response.url);
      form.setFieldsValue({ imageUrl: info.file.response.url });
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

  // 处理清空库存
  const handleClearInventory = () => {
    message.success('清空库存功能开发中');
  };

  // 处理筛选
  const handleFilter = () => {
    setCurrentPage(1);
  };

  // 处理下载模板
  const handleDownloadTemplate = () => {
    message.success('下载模板功能开发中');
  };

  // 表格列定义
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
      dataIndex: 'serialNumber',
      key: 'serialNumber',
      width: 120
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
      width: 100
    },
    {
      title: '备注',
      dataIndex: 'remark',
      key: 'remark',
      width: 150
    },
    {
      title: '图片',
      dataIndex: 'image',
      key: 'image',
      width: 80,
      render: (image) => image ? <Image width={40} src={image} /> : '-'
    },
    {
      title: '设备状态',
      dataIndex: 'deviceStatus',
      key: 'deviceStatus',
      width: 100,
      render: (status) => {
        let color = 'green';
        let text = '正常';
        if (status === 2) {
          color = 'orange';
          text = '损坏';
        } else if (status === 3) {
          color = 'red';
          text = '报废';
        }
        return <Tag color={color}>{text}</Tag>;
      }
    },
    {
      title: '使用状态',
      dataIndex: 'useStatus',
      key: 'useStatus',
      width: 100,
      render: (status) => {
        let color = 'green';
        let text = '未使用';
        if (status === 1) {
          color = 'blue';
          text = '使用中';
        }
        return <Tag color={color}>{text}</Tag>;
      }
    },
    {
      title: '所属公司',
      dataIndex: 'company',
      key: 'company',
      width: 150
    },
    {
      title: '所在仓库',
      dataIndex: 'location',
      key: 'location',
      width: 150
    },
    {
      title: '操作',
      key: 'action',
      width: 120,
      render: (_, record) => {
        return (
          <Space size="middle">
            <Button type="primary" icon={<EditOutlined />} onClick={() => handleEditEquipment(record)} />
            <Popconfirm
              title="确定要删除这个专用设备吗？"
              onConfirm={() => handleDeleteEquipment(record.id)}
              okText="确定"
              cancelText="取消"
            >
              <Button danger icon={<DeleteOutlined />} />
            </Popconfirm>
          </Space>
        );
      }
    }
  ];

  return (
    <div className="special-equipment-management">
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
              placeholder="设备状态"
              value={deviceStatus}
              onChange={(value) => setDeviceStatus(value)}
              style={{ width: 120 }}
              allowClear
            >
              <Option value="1">正常</Option>
              <Option value="2">损坏</Option>
              <Option value="3">报废</Option>
            </Select>
            <Select
              placeholder="使用状态"
              value={useStatus}
              onChange={(value) => setUseStatus(value)}
              style={{ width: 120 }}
              allowClear
            >
              <Option value="1">使用中</Option>
              <Option value="2">未使用</Option>
            </Select>
            <Input
              placeholder="品牌"
              value={brand}
              onChange={(e) => setBrand(e.target.value)}
              style={{ width: 120 }}
              allowClear
            />
            <Button type="primary" onClick={handleFilter}>筛选</Button>
            <Button icon={<ImportOutlined />} onClick={handleImportExcel}>导入Excel</Button>
            {canAction('create') && (
              <Button type="primary" icon={<PlusOutlined />} onClick={handleAddEquipment}>新增设备</Button>
            )}
            <Button danger icon={<DeleteTwoTone />} onClick={handleClearInventory}>清空库存</Button>
          </Space>
        }
      >
        <Table
          columns={columns}
          dataSource={equipments}
          rowKey={(record) => record.id || Math.random().toString(36)}
          loading={loading}
          pagination={{
            current: currentPage,
            pageSize: pageSize,
            total: total,
            onChange: (page, size) => {
              setCurrentPage(page);
              setPageSize(size);
              fetchEquipments(page, size, searchKeyword, deviceStatus, useStatus, brand);
            },
            showSizeChanger: true,
            showQuickJumper: true,
            showTotal: (total) => `共 ${total} 条`
          }}
          scroll={{ x: 2400 }}
        />
      </Card>

      {/* 添加/编辑专用设备模态框 */}
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
                name="deviceCode"
                label="设备编号"
                rules={[{ required: true, message: '请输入设备编号' }]}
              >
                <Input placeholder="请输入设备编号" />
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
                name="serialNumber"
                label="SN码"
              >
                <Input placeholder="请输入SN码" />
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
          </Row>

          <Row gutter={16}>
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
                rules={[{ required: true, message: '请输入单位' }]}
              >
                <Input placeholder="如：台、个" />
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item
                name="deviceStatus"
                label="设备状态"
                rules={[{ required: true, message: '请选择设备状态' }]}
              >
                <Select defaultValue={1}>
                  <Option value={1}>正常</Option>
                  <Option value={2}>损坏</Option>
                  <Option value={3}>报废</Option>
                </Select>
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={16}>
            <Col span={6}>
              <Form.Item
                name="useStatus"
                label="使用状态"
                rules={[{ required: true, message: '请选择使用状态' }]}
              >
                <Select defaultValue={2}>
                  <Option value={1}>使用中</Option>
                  <Option value={2}>未使用</Option>
                </Select>
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item
                name="company"
                label="所属公司"
                rules={[{ required: true, message: '请输入所属公司' }]}
              >
                <Input placeholder="请输入所属公司" />
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item
                name="location"
                label="所在仓库"
                rules={[{ required: true, message: '请输入所在仓库' }]}
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
          </Row>

          <Form.Item>
            <Space style={{ float: 'right' }}>
              <Button onClick={() => setModalVisible(false)}>Cancel</Button>
              <Button type="primary" htmlType="submit">OK</Button>
            </Space>
          </Form.Item>
        </Form>
      </Modal>

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
            <p>请上传包含专用设备信息的Excel文件，支持.xls和.xlsx格式</p>
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

export default SpecialEquipmentManagement