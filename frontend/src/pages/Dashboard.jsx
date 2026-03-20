import React, { useState, useEffect } from 'react';
import { Card, Row, Col, Statistic, Table, Tag } from 'antd';
import { 
  DatabaseOutlined, 
  InboxOutlined, 
  MailOutlined, 
  WarningOutlined 
} from '@ant-design/icons';
import dashboardApi from '../api/dashboard';

const Dashboard = () => {
  const [dashboardData, setDashboardData] = useState({
    totalConsumables: 0,
    totalGeneralEquipments: 0,
    totalSpecialEquipments: 0,
    totalEquipment: 0,
    totalProjectInbounds: 0,
    totalProjectOutbounds: 0,
    lowStockItems: 0,
    recentActivities: []
  });
  const [loading, setLoading] = useState(true);

  const fetchDashboardData = async () => {
    setLoading(true);
    try {
      const overviewResponse = await dashboardApi.getOverview();
      
      if (overviewResponse && overviewResponse.success) {
        const data = overviewResponse.data;
        
        const recentActivities = [
          ...(data.RecentInbounds || []).map(item => ({
            key: `inbound-${item.Id}`,
            id: item.Id,
            type: item.Type,
            projectName: item.ProjectName,
            status: item.Status,
            time: item.CreatedAt ? new Date(item.CreatedAt).toLocaleString() : ''
          })),
          ...(data.RecentOutbounds || []).map(item => ({
            key: `outbound-${item.Id}`,
            id: item.Id,
            type: item.Type,
            projectName: item.ProjectName,
            status: item.Status,
            time: item.CreatedAt ? new Date(item.CreatedAt).toLocaleString() : ''
          }))
        ].sort((a, b) => new Date(b.time) - new Date(a.time)).slice(0, 5);

        setDashboardData({
          totalConsumables: data.TotalConsumables || 0,
          totalGeneralEquipments: data.TotalGeneralEquipments || 0,
          totalSpecialEquipments: data.TotalSpecialEquipments || 0,
          totalEquipment: (data.TotalGeneralEquipments || 0) + (data.TotalSpecialEquipments || 0),
          totalProjectInbounds: data.TotalProjectInbounds || 0,
          totalProjectOutbounds: data.TotalProjectOutbounds || 0,
          lowStockItems: data.LowStockItems || 0,
          recentActivities
        });
      }
    } catch (error) {
      console.error('获取仪表盘数据失败:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchDashboardData();
  }, []);

  const columns = [
    {
      title: '类型',
      dataIndex: 'type',
      key: 'type',
      width: 100,
      render: (type) => {
        let color = 'blue';
        if (type === '入库') color = 'green';
        else if (type === '出库') color = 'orange';
        return <Tag color={color}>{type}</Tag>;
      }
    },
    {
      title: '项目名称',
      dataIndex: 'projectName',
      key: 'projectName',
      width: 200
    },
    {
      title: '状态',
      dataIndex: 'status',
      key: 'status',
      width: 100,
      render: (status) => {
        let color = 'blue';
        if (status === '已完成') color = 'green';
        else if (status === '进行中') color = 'orange';
        return <Tag color={color}>{status}</Tag>;
      }
    },
    {
      title: '时间',
      dataIndex: 'time',
      key: 'time',
      width: 200
    }
  ];

  return (
    <div className="dashboard">
      <Row gutter={16} style={{ marginBottom: 24 }}>
        <Col span={6}>
          <Card>
            <Statistic
              title="设备总数"
              value={dashboardData.totalEquipment}
              prefix={<DatabaseOutlined />}
              valueStyle={{ color: '#3f8600' }}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="入库总数"
              value={dashboardData.totalProjectInbounds}
              prefix={<InboxOutlined />}
              valueStyle={{ color: '#52c41a' }}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="出库总数"
              value={dashboardData.totalProjectOutbounds}
              prefix={<MailOutlined />}
              valueStyle={{ color: '#fa8c16' }}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="库存预警"
              value={dashboardData.lowStockItems}
              prefix={<WarningOutlined />}
              valueStyle={{ color: '#cf1322' }}
            />
          </Card>
        </Col>
      </Row>

      <Card title="最近添加的设备">
        <Table
          columns={columns}
          dataSource={dashboardData.recentActivities}
          rowKey={(record) => record.Id || record.id || record.key || Math.random().toString(36)}
          loading={loading}
          pagination={false}
          scroll={{ x: 600 }}
        />
      </Card>
    </div>
  );
};

export default Dashboard