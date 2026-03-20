import React, { useState, useEffect, useCallback } from 'react';
import { Card, Table, Input, Button, Tag, Statistic, Row, Col, message, Modal, Form, Select, InputNumber, Tabs, Space } from 'antd';
import { SearchOutlined, WarningOutlined, DatabaseOutlined, ReloadOutlined, FileTextOutlined, SettingOutlined, CheckCircleOutlined, AlertOutlined, BarChartOutlined } from '@ant-design/icons';
import inventoryApi from '../api/inventory';
import { debounce } from '../utils/cache';
import './InventoryManagement.css';

const { Option } = Select;

const InventoryManagement = () => {
  const [devices, setDevices] = useState([]);
  const [loading, setLoading] = useState(false);
  const [searchKeyword, setSearchKeyword] = useState('');
  const [stats, setStats] = useState({
    totalDevices: 0,
    lowStock: 0,
    zeroStock: 0,
    totalValue: 0
  });
  const [预警ModalVisible, set预警ModalVisible] = useState(false);
  const [盘点ModalVisible, set盘点ModalVisible] = useState(false);
  const [reportModalVisible, setReportModalVisible] = useState(false);
  const [selectedCategory, setSelectedCategory] = useState('all');
  const [预警设置, set预警设置] = useState({
    lowStockThreshold: 10,
    zeroStockThreshold: 0
  });
  const [盘点记录, set盘点记录] = useState([]);
  const [current盘点, setCurrent盘点] = useState(null);
  const [form] = Form.useForm();
  const [showSummary, setShowSummary] = useState(true);
  
  // 获取当前用户角色
  const userRole = React.useMemo(() => {
    const userStr = localStorage.getItem('user');
    const user = userStr ? JSON.parse(userStr) : null;
    return user?.role || user?.Role || '';
  }, []);
  
  // 判断是否为游客角色
  const isGuest = userRole === 'Guest';
  
  // 游客角色强制使用汇总视图
  React.useEffect(() => {
    if (isGuest) {
      setShowSummary(true);
      set预警ModalVisible(false);
      set盘点ModalVisible(false);
      setReportModalVisible(false);
    }
  }, [isGuest]);
  
  // 游客角色尝试修改showSummary时强制重置为true
  React.useEffect(() => {
    if (isGuest && !showSummary) {
      setShowSummary(true);
    }
  }, [isGuest, showSummary]);

  const getColumns = () => {
    const baseColumns = [
      {
        title: '设备类型',
        dataIndex: 'deviceType',
        key: 'deviceType',
        width: 100,
        render: (type) => {
          const typeMap = {
            1: '专用设备',
            2: '通用设备',
            3: '耗材',
            4: '原材料'
          };
          return <Tag color="blue">{typeMap[type] || type}</Tag>;
        }
      },
      {
        title: '设备名称',
        dataIndex: 'deviceName',
        key: 'deviceName',
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
        title: '总数量',
        dataIndex: 'quantity',
        key: 'quantity',
        width: 100
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
          else if (num < 预警设置.lowStockThreshold) color = 'orange';
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
        title: '位置',
        dataIndex: 'location',
        key: 'location',
        width: 120
      },
      {
        title: '公司',
        dataIndex: 'company',
        key: 'company',
        width: 150
      },
      {
        title: '状态',
        key: 'status',
        width: 100,
        render: (_, record) => {
          const remaining = record.remainingQuantity;
          if (remaining === 0) {
            return <Tag color="red">缺货</Tag>;
          } else if (remaining < 预警设置.lowStockThreshold) {
            return <Tag color="orange">库存不足</Tag>;
          } else {
            return <Tag color="green">正常</Tag>;
          }
        }
      }
    ];

    // 在详细视图下添加设备编号和SN码列
    if (!showSummary) {
      baseColumns.splice(2, 0, {
        title: '设备编号',
        dataIndex: 'deviceCode',
        key: 'deviceCode',
        width: 120
      });
      baseColumns.splice(4, 0, {
        title: 'SN码',
        dataIndex: 'serialNumber',
        key: 'serialNumber',
        width: 150
      });
    }

    return baseColumns;
  };

  // 按设备名称、品牌、型号汇总数据
  const groupDevicesByNameBrandModel = (deviceList) => {
    const groups = {};
    deviceList.forEach(device => {
      const key = `${device.deviceName || '未知'}_${device.brand || '未知'}_${device.model || '未知'}`;
      if (!groups[key]) {
        groups[key] = {
          ...device,
          quantity: 0,
          usedQuantity: 0,
          remainingQuantity: 0,
          deviceCode: '',
          id: `group_${key}`
        };
      }
      groups[key].quantity += device.quantity || 0;
      groups[key].usedQuantity += device.usedQuantity || 0;
      groups[key].remainingQuantity += device.remainingQuantity || 0;
    });
    return Object.values(groups);
  };

  const fetchAllInventory = async () => {
    setLoading(true);
    try {
      const params = {
        category: selectedCategory !== 'all' ? selectedCategory : undefined
      };
      
      const response = await inventoryApi.getInventory(params);
      
      if (Array.isArray(response)) {
        let allDevices = response.map(item => {
          const deviceType = item.category === '专用设备' ? 1 : item.category === '通用设备' ? 2 : item.category === '耗材' ? 3 : item.category === '原材料' ? 4 : 0;
          const isInUse = (deviceType === 1 || deviceType === 2) && item.useStatus === 1;
          return {
            ...item,
            deviceName: item.equipmentName || item.deviceName || '未知设备',
            deviceType: deviceType,
            quantity: item.currentQuantity || item.quantity || 0,
            remainingQuantity: isInUse ? (item.currentQuantity || item.quantity || 0) - 1 : (item.currentQuantity || item.quantity || 0),
            usedQuantity: isInUse ? 1 : 0,
            brand: item.brand || '',
            model: item.model || '',
            deviceCode: item.equipmentCode || item.deviceCode || '',
            serialNumber: item.serialNumber || '',
            unit: item.unit || '',
            location: item.location || '',
            company: item.company || ''
          };
        });
        
        if (showSummary) {
          allDevices = groupDevicesByNameBrandModel(allDevices);
        }
        
        setDevices(allDevices);

        setStats({
          totalDevices: allDevices.length,
          lowStock: allDevices.filter(d => d.remainingQuantity > 0 && d.remainingQuantity < 预警设置.lowStockThreshold).length,
          zeroStock: allDevices.filter(d => d.remainingQuantity === 0).length,
          totalValue: allDevices.reduce((sum, item) => sum + (item.quantity || 0), 0)
        });
      } else {
        message.error('获取库存数据格式错误');
      }
    } catch (error) {
      message.error('获取库存数据失败');
      console.error('获取库存数据失败:', error);
      setDevices([]);
      setStats({
        totalDevices: 0,
        lowStock: 0,
        zeroStock: 0,
        totalValue: 0
      });
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchAllInventory();
  }, [selectedCategory, 预警设置, showSummary]);

  const handleSearch = useCallback(debounce(() => {
    if (!searchKeyword) {
      fetchAllInventory();
      return;
    }
    const filtered = devices.filter(device =>
      device.deviceName.toLowerCase().includes(searchKeyword.toLowerCase()) ||
      device.deviceCode.toLowerCase().includes(searchKeyword.toLowerCase()) ||
      (device.brand && device.brand.toLowerCase().includes(searchKeyword.toLowerCase())) ||
      (device.model && device.model.toLowerCase().includes(searchKeyword.toLowerCase()))
    );
    setDevices(filtered);
  }, 300), [searchKeyword, devices]);

  const showLowStock = () => {
    const filtered = devices.filter(d => d.remainingQuantity > 0 && d.remainingQuantity < 预警设置.lowStockThreshold);
    setDevices(filtered);
  };

  const showZeroStock = () => {
    const filtered = devices.filter(d => d.remainingQuantity === 0);
    setDevices(filtered);
  };

  const showAll = () => {
    fetchAllInventory();
  };

  const handle预警设置 = async (values) => {
    set预警设置(values);
    set预警ModalVisible(false);
    message.success('预警设置已更新');
  };

  const handle开始盘点 = () => {
    setCurrent盘点({
      id: Date.now(),
      date: new Date().toLocaleDateString(),
      items: devices.map(item => ({
        ...item,
        盘点数量: item.remainingQuantity,
        差异: 0
      }))
    });
    set盘点ModalVisible(true);
  };

  const handle盘点确认 = () => {
    if (current盘点) {
      const 差异物品 = current盘点.items.filter(item => item.差异 !== 0);
      if (差异物品.length > 0) {
        message.warning(`发现 ${差异物品.length} 项差异，需要调整库存`);
      } else {
        message.success('盘点完成，无差异');
      }
      set盘点记录([...盘点记录, current盘点]);
      set盘点ModalVisible(false);
    }
  };

  const handle盘点数量变更 = (index, value) => {
    if (current盘点) {
      const newItems = [...current盘点.items];
      newItems[index].盘点数量 = value;
      newItems[index].差异 = value - newItems[index].remainingQuantity;
      setCurrent盘点({
        ...current盘点,
        items: newItems
      });
    }
  };

  return (
    <div className="inventory-management">
      <Row gutter={16} style={{ marginBottom: 24 }}>
        <Col span={6}>
          <Card>
            <Statistic
              title="库存总数"
              value={stats.totalDevices}
              prefix={<DatabaseOutlined />}
              valueStyle={{ color: '#3f8600' }}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="库存不足"
              value={stats.lowStock}
              prefix={<WarningOutlined />}
              valueStyle={{ color: '#fa8c16' }}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="缺货"
              value={stats.zeroStock}
              prefix={<AlertOutlined />}
              valueStyle={{ color: '#cf1322' }}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="总数量"
              value={stats.totalValue}
              prefix={<BarChartOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
      </Row>

      <Card
        title="库存管理"
        extra={
          <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap', alignItems: 'center' }}>
            <Select
              defaultValue="all"
              onChange={setSelectedCategory}
              style={{ width: 120 }}
            >
              <Option value="all">全部类型</Option>
              <Option value="1">专用设备</Option>
              <Option value="2">通用设备</Option>
              <Option value="3">耗材</Option>
              <Option value="4">原材料</Option>
            </Select>
            <Button onClick={showAll} icon={<ReloadOutlined />}>全部</Button>
            {!isGuest && (
              <>
                <Button onClick={showLowStock} type="primary" ghost>库存不足</Button>
                <Button onClick={showZeroStock} danger>缺货</Button>
                <Button onClick={() => setShowSummary(!showSummary)} type={showSummary ? 'primary' : 'default'}>
                  {showSummary ? '详细视图' : '汇总视图'}
                </Button>
                <Button onClick={() => set预警ModalVisible(true)} icon={<SettingOutlined />}>预警设置</Button>
                <Button onClick={handle开始盘点} icon={<CheckCircleOutlined />}>盘点</Button>
                <Button onClick={() => setReportModalVisible(true)} icon={<FileTextOutlined />}>报表</Button>
              </>
            )}
            <Input.Search
              placeholder="搜索设备"
              allowClear
              style={{ width: 200 }}
              onSearch={handleSearch}
              onChange={(e) => {
                setSearchKeyword(e.target.value);
                handleSearch();
              }}
            />
          </div>
        }
      >
        <Table
          columns={getColumns()}
          dataSource={devices}
          rowKey={(record) => record.Id || record.id || Math.random().toString(36)}
          loading={loading}
          pagination={{ 
            pageSize: 10,
            showSizeChanger: true,
            pageSizeOptions: ['10', '20', '50'],
            showTotal: (total) => `共 ${total} 条记录`
          }}
          scroll={{ x: 1500 }}
        />
      </Card>

      {/* 预警设置模态框 */}
      <Modal
        title="库存预警设置"
        open={预警ModalVisible}
        onOk={() => form.submit()}
        onCancel={() => set预警ModalVisible(false)}
        width={400}
        okText="OK"
        cancelText="Cancel"
      >
        <Form
          form={form}
          layout="vertical"
          initialValues={预警设置}
          onFinish={handle预警设置}
        >
          <Form.Item
            label="库存不足阈值"
            name="lowStockThreshold"
            rules={[{ required: true, message: '请输入库存不足阈值' }]}
          >
            <InputNumber min={1} max={100} style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item
            label="缺货阈值"
            name="zeroStockThreshold"
            rules={[{ required: true, message: '请输入缺货阈值' }]}
          >
            <InputNumber min={0} max={10} style={{ width: '100%' }} />
          </Form.Item>
        </Form>
      </Modal>

      {/* 盘点模态框 */}
      <Modal
        title="库存盘点"
        open={盘点ModalVisible}
        onOk={handle盘点确认}
        onCancel={() => set盘点ModalVisible(false)}
        width={800}
        okText="确认盘点"
      >
        {current盘点 && (
          <Table
            columns={[
              { title: '设备编号', dataIndex: 'deviceCode' },
              { title: '设备名称', dataIndex: 'deviceName' },
              { title: '当前库存', dataIndex: 'remainingQuantity' },
              {
                title: '盘点数量',
                dataIndex: '盘点数量',
                render: (value, record, index) => (
                  <InputNumber
                    min={0}
                    value={value}
                    onChange={(val) => handle盘点数量变更(index, val)}
                    style={{ width: 100 }}
                  />
                )
              },
              {
                title: '差异',
                dataIndex: '差异',
                render: (value) => (
                  <Tag color={value === 0 ? 'green' : value > 0 ? 'blue' : 'red'}>
                    {value > 0 ? `+${value}` : value}
                  </Tag>
                )
              }
            ]}
            dataSource={current盘点.items}
            rowKey={(record) => record.Id || record.id || Math.random().toString(36)}
            pagination={{ pageSize: 10 }}
          />
        )}
      </Modal>

      {/* 报表模态框 */}
      <Modal
        title="库存报表"
        open={reportModalVisible}
        onCancel={() => setReportModalVisible(false)}
        width={900}
        footer={[
          <Button key="close" onClick={() => setReportModalVisible(false)}>
            关闭
          </Button>
        ]}
      >
        <Tabs defaultActiveKey="summary">
          <Tabs.TabPane tab="库存汇总" key="summary">
            <Row gutter={16} style={{ marginBottom: 24 }}>
              <Col span={8}>
                <Card>
                  <Statistic
                    title="专用设备"
                    value={devices.filter(d => d.deviceType === 1).length}
                    valueStyle={{ color: '#1890ff' }}
                  />
                </Card>
              </Col>
              <Col span={8}>
                <Card>
                  <Statistic
                    title="通用设备"
                    value={devices.filter(d => d.deviceType === 2).length}
                    valueStyle={{ color: '#52c41a' }}
                  />
                </Card>
              </Col>
              <Col span={8}>
                <Card>
                  <Statistic
                    title="耗材"
                    value={devices.filter(d => d.deviceType === 3).length}
                    valueStyle={{ color: '#fa8c16' }}
                  />
                </Card>
              </Col>
            </Row>
            <Row gutter={16} style={{ marginBottom: 24 }}>
              <Col span={8}>
                <Card>
                  <Statistic
                    title="原材料"
                    value={devices.filter(d => d.deviceType === 4).length}
                    valueStyle={{ color: '#722ed1' }}
                  />
                </Card>
              </Col>
              <Col span={8}>
                <Card>
                  <Statistic
                    title="库存不足"
                    value={stats.lowStock}
                    valueStyle={{ color: '#fa8c16' }}
                  />
                </Card>
              </Col>
              <Col span={8}>
                <Card>
                  <Statistic
                    title="缺货"
                    value={stats.zeroStock}
                    valueStyle={{ color: '#cf1322' }}
                  />
                </Card>
              </Col>
            </Row>
          </Tabs.TabPane>
          <Tabs.TabPane tab="库存详情" key="details">
            <Table
              columns={[
                { title: '设备编号', dataIndex: 'deviceCode' },
                { title: '设备名称', dataIndex: 'deviceName' },
                { title: '设备类型', dataIndex: 'deviceType', render: (type) => {
                  const typeMap = {
                    1: '专用设备',
                    2: '通用设备',
                    3: '耗材',
                    4: '原材料'
                  };
                  return typeMap[type] || type;
                }},
                { title: '总数量', dataIndex: 'quantity' },
                { title: '剩余数量', dataIndex: 'remainingQuantity' },
                { title: '位置', dataIndex: 'location' }
              ]}
              dataSource={devices}
              rowKey={(record) => record.Id || record.id || Math.random().toString(36)}
              pagination={{ pageSize: 10 }}
            />
          </Tabs.TabPane>
          <Tabs.TabPane tab="盘点记录" key="inventory">
            {盘点记录.length > 0 ? (
              <Table
                columns={[
                  { title: '盘点日期', dataIndex: 'date' },
                  { title: '盘点项数', dataIndex: 'items', render: (items) => items.length },
                  { title: '差异项数', dataIndex: 'items', render: (items) => items.filter(item => item.差异 !== 0).length },
                  {
                    title: '操作',
                    render: (_, record) => (
                      <Button type="link">查看详情</Button>
                    )
                  }
                ]}
                dataSource={盘点记录}
                rowKey={(record) => record.Id || record.id || Math.random().toString(36)}
                pagination={{ pageSize: 10 }}
              />
            ) : (
              <div style={{ textAlign: 'center', padding: '40px' }}>
                暂无盘点记录
              </div>
            )}
          </Tabs.TabPane>
        </Tabs>
      </Modal>
    </div>
  );
};

export default InventoryManagement