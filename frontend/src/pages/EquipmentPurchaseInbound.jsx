import React, { useState, useEffect } from 'react'
import { Table, Button, Modal, Form, Input, Select, DatePicker, message, Space, Popconfirm, Tag, Card, InputNumber, Divider, Alert, AutoComplete } from 'antd'
import { PlusOutlined, EditOutlined, DeleteOutlined, EyeOutlined, ReloadOutlined, CheckCircleOutlined } from '@ant-design/icons'
import equipmentInboundApi from '../api/equipmentInbound'
import specialEquipmentApi from '../api/specialEquipment'
import generalEquipmentApi from '../api/generalEquipment'
import consumableApi from '../api/consumable'
import dayjs from 'dayjs'

const { Option } = Select
const { TextArea } = Input

function EquipmentPurchaseInbound() {
  const [form] = Form.useForm()
  const [dataSource, setDataSource] = useState([])
  const [loading, setLoading] = useState(false)
  const [modalVisible, setModalVisible] = useState(false)
  const [detailModalVisible, setDetailModalVisible] = useState(false)
  const [editingRecord, setEditingRecord] = useState(null)
  const [detailRecord, setDetailRecord] = useState(null)
  const [currentPage, setCurrentPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [total, setTotal] = useState(0)
  const [previewRecord, setPreviewRecord] = useState(null)
  const [previewModalVisible, setPreviewModalVisible] = useState(false)
  const [specialEquipments, setSpecialEquipments] = useState([])
  const [generalEquipments, setGeneralEquipments] = useState([])
  const [consumables, setConsumables] = useState([])
  const [notification, setNotification] = useState(null)
  const [equipmentType, setEquipmentType] = useState(1)
  const [equipmentOptions, setEquipmentOptions] = useState([])
  const [consumableOptions, setConsumableOptions] = useState([])
  const [searching, setSearching] = useState(false)
  const [isMounted, setIsMounted] = useState(true)
  
  // 组件卸载时设置isMounted为false
  useEffect(() => {
    return () => {
      setIsMounted(false)
    }
  }, [])

  // 加载设备采购入库数据
  const loadEquipmentPurchaseInbounds = async () => {
    setLoading(true)
    try {
      const response = await equipmentInboundApi.getEquipmentInbounds()
      if (!isMounted) return // 组件已卸载，不更新状态
      if (response.success && response.data) {
        // 确保dataSource是数组
        if (Array.isArray(response.data)) {
          setDataSource(response.data)
          setTotal(response.data.length)
        } else if (response.data.data && Array.isArray(response.data.data)) {
          setDataSource(response.data.data)
          setTotal(response.data.data.length)
        } else {
          setDataSource([])
          setTotal(0)
        }
      } else {
        setDataSource([])
        setTotal(0)
      }
    } catch (error) {
      if (!isMounted) return // 组件已卸载，不更新状态
      message.error('加载设备采购入库数据失败')
      console.error('Error loading equipment inbounds:', error)
      setDataSource([])
      setTotal(0)
    } finally {
      if (isMounted) {
        setLoading(false)
      }
    }
  }

  useEffect(() => {
    loadEquipmentPurchaseInbounds()
  }, [])

  // 检查入库单编号是否存在
  const checkInboundNumberExists = async (number) => {
    if (!isMounted) return false // 组件已卸载，直接返回
    try {
      const response = await equipmentInboundApi.checkEquipmentInboundExists(number)
      if (!isMounted) return false // 组件已卸载，直接返回
      if (response.success) {
        return response.data
      }
      return false
    } catch (error) {
      console.error('检查入库单编号失败:', error)
      return false
    }
  }

  // 打开添加/编辑模态框
  const openModal = (record = null) => {
    setEditingRecord(record)
    if (record) {
      setSpecialEquipments(record.specialEquipments || [])
      setGeneralEquipments(record.generalEquipments || [])
      setConsumables(record.consumables || [])
    } else {
      setSpecialEquipments([])
      setGeneralEquipments([])
      setConsumables([])
      setNotification(null)
    }
    setModalVisible(true)
    
    // 延迟执行form操作，确保Form组件已经挂载
    setTimeout(() => {
      if (record) {
        form.setFieldsValue({
          ...record,
          inboundDate: record.inboundDate ? dayjs(record.inboundDate) : null,
          equipmentType: record.equipmentType
        })
      } else {
        // 生成入库单号
        const generateInboundNumber = async () => {
          let inboundNumber
          let exists
          do {
            const today = new Date()
            const year = today.getFullYear()
            const month = String(today.getMonth() + 1).padStart(2, '0')
            const day = String(today.getDate()).padStart(2, '0')
            const random = Math.floor(Math.random() * 10000).toString().padStart(4, '0')
            inboundNumber = `XZSB${year}${month}${day}${random}`
            exists = await checkInboundNumberExists(inboundNumber)
          } while (exists)
          
          form.resetFields()
          form.setFieldsValue({
            inboundNumber,
            inboundDate: dayjs(),
            equipmentType: 1, // 默认专用设备
            deviceName: '',
            brand: '',
            model: '',
            quantity: 1,
            unit: '台',
            status: '正常',
            consumableName: '',
            modelSpec: '',
            consumableUnit: '件',
            consumableRemark: ''
          })
        }
        
        generateInboundNumber()
      }
    }, 0)
  }

  // 打开详情模态框
  const openDetailModal = async (record) => {
    if (!isMounted) return // 组件已卸载，直接返回
    try {
      const response = await equipmentInboundApi.getEquipmentInbound(record.id)
      if (!isMounted) return // 组件已卸载，直接返回
      if (response.success && response.data) {
        setDetailRecord(response.data)
        setDetailModalVisible(true)
      }
    } catch (error) {
      if (!isMounted) return // 组件已卸载，直接返回
      message.error('获取入库单详情失败')
      console.error('Error loading equipment inbound detail:', error)
    }
  }

  // 打开预览模态框
  const openPreviewModal = async (record) => {
    if (!isMounted) return // 组件已卸载，直接返回
    try {
      const response = await equipmentInboundApi.getEquipmentInbound(record.id)
      if (!isMounted) return // 组件已卸载，直接返回
      if (response.success && response.data) {
        setPreviewRecord(response.data)
        setPreviewModalVisible(true)
      }
    } catch (error) {
      if (!isMounted) return // 组件已卸载，直接返回
      message.error('获取入库单详情失败')
      console.error('Error loading equipment inbound detail:', error)
    }
  }

  // 关闭模态框
  const closeModal = () => {
    setModalVisible(false)
    setEditingRecord(null)
    form.resetFields()
    setSpecialEquipments([])
    setGeneralEquipments([])
    setConsumables([])
    setNotification(null)
  }

  // 关闭详情模态框
  const closeDetailModal = () => {
    setDetailModalVisible(false)
    setDetailRecord(null)
  }

  // 关闭预览模态框
  const closePreviewModal = () => {
    setPreviewModalVisible(false)
    setPreviewRecord(null)
  }

  // 保存设备采购入库
  const saveEquipmentPurchaseInbound = async () => {
    if (!isMounted) return // 组件已卸载，直接返回
    try {
      const values = await form.validateFields()
      
      // 转换日期格式
      const inboundDate = values.inboundDate ? values.inboundDate.toISOString() : null
      const equipmentType = parseInt(values.equipmentType)
      
      // 构建Items数组
      const items = []
      
      // 添加专用设备
      specialEquipments.forEach(equipment => {
        items.push({
          DeviceName: equipment.deviceName,
          DeviceCode: equipment.deviceCode,
          Brand: equipment.brand,
          Model: equipment.model,
          SerialNumber: equipment.serialNumber,
          Quantity: equipment.quantity,
          Unit: equipment.unit,
          ImageUrl: '',
          Status: equipment.status,
          Remark: equipment.remark,
          EquipmentType: 1
        })
      })
      
      // 添加通用设备
      generalEquipments.forEach(equipment => {
        items.push({
          DeviceName: equipment.deviceName,
          DeviceCode: equipment.deviceCode,
          Brand: equipment.brand,
          Model: equipment.model,
          SerialNumber: equipment.serialNumber,
          Quantity: equipment.quantity,
          Unit: equipment.unit,
          ImageUrl: '',
          Status: equipment.status,
          Remark: equipment.remark,
          EquipmentType: 2
        })
      })
      
      // 添加耗材
      consumables.forEach(consumable => {
        items.push({
          DeviceName: consumable.consumableName,
          DeviceCode: `耗材-${Date.now()}-${Math.floor(Math.random() * 1000)}`,
          Brand: consumable.brand,
          Model: consumable.modelSpec,
          SerialNumber: '',
          Specification: '',
          Quantity: consumable.quantity,
          Unit: consumable.unit,
          ImageUrl: '',
          Status: '',
          Remark: consumable.remark,
          EquipmentType: 3
        })
      })
      
      // 构建请求数据
      const formattedValues = {
        InboundNumber: values.inboundNumber,
        InboundDate: inboundDate,
        EquipmentType: equipmentType,
        DeliveryPerson: values.deliveryPerson,
        Operator: values.operator,
        Remark: values.remark,
        Items: items
      }
      
      if (editingRecord) {
        await equipmentInboundApi.updateEquipmentInbound(editingRecord.id, formattedValues)
        if (!isMounted) return // 组件已卸载，直接返回
        message.success('更新设备采购入库成功')
      } else {
        await equipmentInboundApi.createEquipmentInbound(formattedValues)
        if (!isMounted) return // 组件已卸载，直接返回
        message.success('创建设备采购入库成功')
      }
      
      if (!isMounted) return // 组件已卸载，直接返回
      closeModal()
      loadEquipmentPurchaseInbounds()
    } catch (error) {
      if (!isMounted) return // 组件已卸载，直接返回
      message.error('保存失败，请检查输入')
      console.error('Error saving equipment inbound:', error)
    }
  }

  // 删除设备采购入库
  const deleteEquipmentPurchaseInbound = async (id) => {
    if (!isMounted) return // 组件已卸载，直接返回
    try {
      await equipmentInboundApi.deleteEquipmentInbound(id)
      if (!isMounted) return // 组件已卸载，直接返回
      message.success('删除设备采购入库成功')
      loadEquipmentPurchaseInbounds()
    } catch (error) {
      if (!isMounted) return // 组件已卸载，直接返回
      message.error('删除失败')
      console.error('Error deleting equipment inbound:', error)
    }
  }

  // 完成设备采购入库
  const completeEquipmentPurchaseInbound = async (id) => {
    if (!isMounted) return // 组件已卸载，直接返回
    try {
      await equipmentInboundApi.completeEquipmentInbound(id)
      if (!isMounted) return // 组件已卸载，直接返回
      message.success('入库单完成成功')
      loadEquipmentPurchaseInbounds()
    } catch (error) {
      if (!isMounted) return // 组件已卸载，直接返回
      message.error('完成失败')
      console.error('Error completing equipment inbound:', error)
    }
  }

  // 新增设备
  const addEquipment = async () => {
    if (!isMounted) return // 组件已卸载，直接返回
    try {
      const values = await form.validateFields()
      
      if (equipmentType === 1) {
        // 专用设备
        const quantity = parseInt(values.quantity) || 1
        const newEquipments = []
        
        // 生成第一个设备编号
        const firstDeviceCode = await generateDeviceCode(values.deviceName, values.brand, values.model, 1)
        if (!isMounted) return // 组件已卸载，直接返回
        const match = firstDeviceCode.match(/YD-(.*)-(\d+)/)
        let baseNumber = 1
        
        if (match) {
          baseNumber = parseInt(match[2])
        }
        
        const deviceNameForCode = values.deviceName.trim()
        
        for (let i = 0; i < quantity; i++) {
          // 为每条记录生成连续递增的设备编号
          const currentNumber = baseNumber + i
          const paddedNumber = String(currentNumber).padStart(3, '0')
          const deviceCode = `YD-${deviceNameForCode}-${paddedNumber}`
          
          const newEquipment = {
            deviceName: values.deviceName,
            brand: values.brand,
            model: values.model,
            quantity: 1, // 每条记录数量为1
            unit: values.unit,
            status: values.status,
            deviceCode: deviceCode,
            serialNumber: '',
            accessories: '',
            remark: ''
          }
          newEquipments.push(newEquipment)
        }
        
        if (!isMounted) return // 组件已卸载，直接返回
        setSpecialEquipments([...specialEquipments, ...newEquipments])
        setNotification({ type: 'success', message: `已添加 ${quantity} 条专用设备记录` })
      } else if (equipmentType === 2) {
        // 通用设备
        const quantity = parseInt(values.quantity) || 1
        const newEquipments = []
        
        // 生成第一个设备编号
        const firstDeviceCode = await generateDeviceCode(values.deviceName, values.brand, values.model, 2)
        if (!isMounted) return // 组件已卸载，直接返回
        const match = firstDeviceCode.match(/YD-(.*)-(\d+)/)
        let baseNumber = 1
        
        if (match) {
          baseNumber = parseInt(match[2])
        }
        
        const deviceNameForCode = values.deviceName.trim()
        
        for (let i = 0; i < quantity; i++) {
          // 为每条记录生成连续递增的设备编号
          const currentNumber = baseNumber + i
          const paddedNumber = String(currentNumber).padStart(3, '0')
          const deviceCode = `YD-${deviceNameForCode}-${paddedNumber}`
          
          const newEquipment = {
            deviceName: values.deviceName,
            brand: values.brand,
            model: values.model,
            quantity: 1, // 每条记录数量为1
            unit: values.unit,
            status: values.status,
            deviceCode: deviceCode,
            serialNumber: '',
            accessories: '',
            remark: ''
          }
          newEquipments.push(newEquipment)
        }
        
        if (!isMounted) return // 组件已卸载，直接返回
        setGeneralEquipments([...generalEquipments, ...newEquipments])
        setNotification({ type: 'success', message: `已添加 ${quantity} 条通用设备记录` })
      } else if (equipmentType === 3) {
        // 耗材
        const newConsumable = {
          consumableName: values.consumableName,
          brand: values.brand,
          modelSpec: values.modelSpec,
          quantity: values.quantity,
          unit: values.consumableUnit,
          remark: values.consumableRemark
        }
        if (!isMounted) return // 组件已卸载，直接返回
        setConsumables([...consumables, newConsumable])
        setNotification({ type: 'success', message: `已添加 ${values.quantity} 条耗材记录` })
      }
      
      // 重置表单
      if (equipmentType === 3) {
        form.setFieldsValue({
          consumableName: '',
          brand: '',
          modelSpec: '',
          quantity: 1,
          consumableUnit: '件',
          consumableRemark: ''
        })
      } else {
        form.setFieldsValue({
          deviceName: '',
          brand: '',
          model: '',
          quantity: 1,
          unit: '台',
          status: '正常'
        })
      }
    } catch (error) {
      if (!isMounted) return // 组件已卸载，直接返回
      message.error('添加设备失败，请检查输入')
      console.error('Error adding equipment:', error)
    }
  }

  // 生成设备编号
  const generateDeviceCode = async (deviceName, brand, model, type) => {
    if (!isMounted) return `YD-${deviceName?.trim() || 'UNK'}-001` // 组件已卸载，返回默认编号
    try {
      // 确保设备名称不为空
      if (!deviceName) {
        return 'YD-UNK-001'
      }
      
      // 使用完整的设备名称
      const deviceNameForCode = deviceName.trim()
      
      // 首先尝试使用getAll方法获取所有设备
      let response
      let allEquipments = []
      const equipmentTypeToUse = type || equipmentType
      
      console.log('开始生成设备编号，设备名称:', deviceName, '设备类型:', equipmentTypeToUse)
      
      // 直接使用fetch API调用后端，先尝试搜索特定设备
      try {
        console.log('1. 尝试使用fetch搜索特定设备...')
        const searchUrl = equipmentTypeToUse === 1 ? `http://localhost:5000/api/SpecialEquipments/search/${encodeURIComponent(deviceName)}` : `http://localhost:5000/api/GeneralEquipments/search/${encodeURIComponent(deviceName)}`
        const searchResponse = await fetch(searchUrl)
        if (!isMounted) return `YD-${deviceNameForCode}-001` // 组件已卸载，返回默认编号
        const searchData = await searchResponse.json()
        console.log('1.1 搜索响应:', searchData)
        if (searchData && searchData.success && searchData.data) {
          allEquipments = Array.isArray(searchData.data) ? searchData.data : searchData.data.items || []
          console.log('1.2 搜索设备成功，数量:', allEquipments.length)
        } else {
          console.error('1.3 搜索设备失败，响应:', searchData)
          // 如果搜索失败，尝试获取所有设备
          try {
            console.log('2. 尝试使用fetch获取所有设备...')
            const allUrl = equipmentTypeToUse === 1 ? 'http://localhost:5000/api/SpecialEquipments' : 'http://localhost:5000/api/GeneralEquipments'
            const allResponse = await fetch(allUrl)
            if (!isMounted) return `YD-${deviceNameForCode}-001` // 组件已卸载，返回默认编号
            const allData = await allResponse.json()
            console.log('2.1 获取所有设备响应:', allData)
            if (allData && allData.success && allData.data) {
              allEquipments = Array.isArray(allData.data) ? allData.data : allData.data.items || []
              console.log('2.2 获取所有设备成功，数量:', allEquipments.length)
            } else {
              console.error('2.3 获取所有设备列表失败，响应:', allData)
            }
          } catch (error) {
            console.error('2.4 获取所有设备失败:', error)
          }
        }
      } catch (error) {
        console.error('1.4 搜索设备失败:', error)
        // 尝试使用API对象
        try {
          console.log('3. 尝试使用API对象搜索设备...')
          if (equipmentTypeToUse === 1) {
            response = await specialEquipmentApi.search(deviceName)
          } else {
            response = await generalEquipmentApi.search(deviceName)
          }
          if (!isMounted) return `YD-${deviceNameForCode}-001` // 组件已卸载，返回默认编号
          console.log('3.1 API搜索响应:', response)
          if (response && response.success && response.data) {
            allEquipments = Array.isArray(response.data) ? response.data : response.data.items || []
            console.log('3.2 API搜索设备成功，数量:', allEquipments.length)
          } else {
            console.error('3.3 API搜索设备失败，响应:', response)
            // 尝试使用API对象获取所有设备
            try {
              console.log('4. 尝试使用API对象获取所有设备...')
              if (equipmentTypeToUse === 1) {
                response = await specialEquipmentApi.getAll()
              } else {
                response = await generalEquipmentApi.getAll()
              }
              if (!isMounted) return `YD-${deviceNameForCode}-001` // 组件已卸载，返回默认编号
              console.log('4.1 API获取所有设备响应:', response)
              if (response && response.success && response.data) {
                allEquipments = Array.isArray(response.data) ? response.data : response.data.items || []
                console.log('4.2 API获取所有设备成功，数量:', allEquipments.length)
              } else {
                console.error('4.3 API获取所有设备列表失败，响应:', response)
              }
            } catch (error) {
              console.error('4.4 API获取所有设备失败:', error)
            }
          }
        } catch (error) {
          console.error('3.4 API搜索设备失败:', error)
        }
      }
      
      console.log('设备名称:', deviceName, '品牌:', brand, '型号:', model)
      console.log('设备类型:', equipmentTypeToUse)
      console.log('设备总数:', allEquipments.length)
      
      // 打印所有设备，看看是否有ALGE终点摄像
      console.log('所有设备:', allEquipments)
      
      // 过滤出同设备名称的设备（使用更宽松的匹配）
      const normalizedDeviceName = deviceName.trim().toLowerCase()
      console.log('输入的设备名称:', deviceName)
      console.log('标准化后的设备名称:', normalizedDeviceName)
      
      const similarEquipments = allEquipments.filter(equipment => {
        const eqDeviceName = equipment.deviceName?.trim()?.toLowerCase() || ''
        console.log('设备名称:', equipment.deviceName, '标准化后:', eqDeviceName)
        const nameMatch = eqDeviceName.includes(normalizedDeviceName) || normalizedDeviceName.includes(eqDeviceName)
        
        if (nameMatch) {
          console.log('匹配到设备:', equipment.deviceName, equipment.deviceCode)
        } else {
          console.log('未匹配到设备:', equipment.deviceName, '标准化后:', eqDeviceName, '输入标准化后:', normalizedDeviceName)
        }
        
        return nameMatch
      })
      
      console.log('匹配到的设备数量:', similarEquipments.length)
      
      // 提取设备编号并找出最大值
      let maxNumber = 0
      similarEquipments.forEach(equipment => {
        if (equipment.deviceCode) {
          // 尝试匹配各种格式的编号
          const match = equipment.deviceCode.match(/(\d+)$/)
          if (match) {
            const number = parseInt(match[1])
            if (number > maxNumber) {
              maxNumber = number
              console.log('找到更大的编号:', equipment.deviceCode, '编号值:', number)
            }
          } else {
            console.log('设备编号格式不正确:', equipment.deviceCode)
          }
        }
      })
      
      console.log('最大编号值:', maxNumber)
      
      // 生成新的设备编号
      const newNumber = maxNumber + 1
      const paddedNumber = String(newNumber).padStart(3, '0')
      const newDeviceCode = `YD-${deviceNameForCode}-${paddedNumber}`
      console.log('生成的新设备编号:', newDeviceCode)
      
      return newDeviceCode
    } catch (error) {
      console.error('生成设备编号失败:', error)
      // 出错时使用默认编号
      const deviceNameForCode = deviceName?.trim() || 'UNK'
      const defaultCode = `YD-${deviceNameForCode}-001`
      console.log('使用默认设备编号:', defaultCode)
      return defaultCode
    }
  }

  // 删除设备
  const removeEquipment = (type, index) => {
    if (type === 'special') {
      const newSpecialEquipments = [...specialEquipments]
      newSpecialEquipments.splice(index, 1)
      setSpecialEquipments(newSpecialEquipments)
    } else if (type === 'general') {
      const newGeneralEquipments = [...generalEquipments]
      newGeneralEquipments.splice(index, 1)
      setGeneralEquipments(newGeneralEquipments)
    } else if (type === 'consumable') {
      const newConsumables = [...consumables]
      newConsumables.splice(index, 1)
      setConsumables(newConsumables)
    }
  }

  // 防抖函数
  const debounce = (func, delay) => {
    let timeoutId
    return (...args) => {
      clearTimeout(timeoutId)
      timeoutId = setTimeout(() => func.apply(null, args), delay)
    }
  }

  // 搜索设备（带防抖）
  const searchEquipment = debounce(async (keyword) => {
    if (!isMounted) return // 组件已卸载，不执行搜索
    setSearching(true)
    try {
      let response
      if (equipmentType === 1) {
        // 搜索专用设备
        response = await specialEquipmentApi.search(keyword || '')
        if (!isMounted) return // 组件已卸载，不更新状态
        if (response && response.success && response.data) {
          // 对设备名称进行去重处理
          const uniqueEquipmentMap = new Map()
          response.data.forEach(equipment => {
            if (!uniqueEquipmentMap.has(equipment.deviceName)) {
              uniqueEquipmentMap.set(equipment.deviceName, equipment)
            }
          })
          
          // 转换为数组并创建选项，不限制数量
          const options = Array.from(uniqueEquipmentMap.values())
            .map(equipment => ({
              value: `${equipment.deviceName}-${equipment.id}`,
              label: `${equipment.deviceName} (${equipment.brand || '无品牌'})`,
              equipment: equipment
            }))
          setEquipmentOptions(options)
        } else {
          setEquipmentOptions([])
        }
      } else if (equipmentType === 2) {
        // 搜索通用设备
        response = await generalEquipmentApi.search(keyword || '')
        if (!isMounted) return // 组件已卸载，不更新状态
        if (response && response.success && response.data) {
          // 对设备名称进行去重处理
          const uniqueEquipmentMap = new Map()
          response.data.forEach(equipment => {
            if (!uniqueEquipmentMap.has(equipment.deviceName)) {
              uniqueEquipmentMap.set(equipment.deviceName, equipment)
            }
          })
          
          // 转换为数组并创建选项，不限制数量
          const options = Array.from(uniqueEquipmentMap.values())
            .map(equipment => ({
              value: `${equipment.deviceName}-${equipment.id}`,
              label: `${equipment.deviceName} (${equipment.brand || '无品牌'})`,
              equipment: equipment
            }))
          setEquipmentOptions(options)
        } else {
          setEquipmentOptions([])
        }
      } else if (equipmentType === 3) {
        // 搜索耗材
        response = await consumableApi.searchConsumables(keyword || '')
        if (!isMounted) return // 组件已卸载，不更新状态
        if (response && response.success && response.data) {
          // 检查数据结构，可能在items中
          const consumables = response.data.items || response.data
          // 对耗材名称进行去重处理
          const uniqueConsumableMap = new Map()
          consumables.forEach(consumable => {
            if (!uniqueConsumableMap.has(consumable.name)) {
              uniqueConsumableMap.set(consumable.name, consumable)
            }
          })
          
          // 转换为数组并创建选项，不限制数量
          const options = Array.from(uniqueConsumableMap.values())
            .map(consumable => ({
              value: `${consumable.name}-${consumable.id}`,
              label: `${consumable.name} (${consumable.brand || '无品牌'})`,
              consumable: consumable
            }))
          setConsumableOptions(options)
        } else {
          setConsumableOptions([])
        }
      } else {
        // 非设备类型，不搜索
        setEquipmentOptions([])
        setConsumableOptions([])
        setSearching(false)
        return
      }
    } catch (error) {
      if (!isMounted) return // 组件已卸载，不更新状态
      console.error('搜索设备失败:', error)
      setEquipmentOptions([])
      setConsumableOptions([])
    } finally {
      if (isMounted) {
        setSearching(false)
      }
    }
  }, 300)

  // 处理设备选择
  const handleEquipmentSelect = (value, option) => {
    if (option.equipment) {
      const equipment = option.equipment
      form.setFieldsValue({
        deviceName: equipment.deviceName,
        brand: equipment.brand || '',
        model: equipment.model || '',
        unit: equipment.unit || '台',
        status: equipment.status || '正常'
      })
    } else if (option.consumable) {
      const consumable = option.consumable
      form.setFieldsValue({
        consumableName: consumable.name,
        brand: consumable.brand || '',
        modelSpec: consumable.modelSpecification || '',
        consumableUnit: consumable.unit || '件'
      })
    }
  }

  // 重置表单
  const resetForm = () => {
    if (equipmentType === 3) {
      form.setFieldsValue({
        consumableName: '',
        brand: '',
        modelSpec: '',
        quantity: 1,
        consumableUnit: '件',
        consumableRemark: ''
      })
      setConsumableOptions([])
    } else {
      form.setFieldsValue({
        deviceName: '',
        brand: '',
        model: '',
        quantity: 1,
        unit: '台',
        status: '正常'
      })
      setEquipmentOptions([])
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
        if (!items || items.length === 0) return '-';
        return items.map(item => item.deviceName).join('; ');
      }
    },
    {
      title: '总数量',
      dataIndex: 'totalQuantity',
      key: 'totalQuantity',
      width: 100
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
      width: 200,
      render: (_, record) => (
        <Space size="middle">
          <Button type="primary" onClick={() => openDetailModal(record)}>编辑</Button>
          <Button onClick={() => openPreviewModal(record)}>预览</Button>
          <Button danger onClick={() => deleteEquipmentPurchaseInbound(record.id)}>删除</Button>
          <Button onClick={() => completeEquipmentPurchaseInbound(record.id)}>完成</Button>
        </Space>
      )
    }
  ]

  return (
    <div>
      <Card
        title="设备采购入库管理"
        extra={
          <Button type="primary" icon={<PlusOutlined />} onClick={() => openModal()}>
            新增设备入库
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

      {/* 添加/编辑设备采购入库模态框 */}
      <Modal
        title={editingRecord ? '编辑设备采购入库' : '新增设备采购入库'}
        open={modalVisible}
        onCancel={closeModal}
        footer={[
          <Button key="preview" icon={<EyeOutlined />} onClick={() => {
            form.validateFields().then(values => {
              const inboundDate = values.inboundDate ? values.inboundDate.format('YYYY-MM-DD') : null
              
              // 构建Items数组用于预览
              const items = []
              
              // 添加专用设备
              specialEquipments.forEach(equipment => {
                items.push({
                  DeviceName: equipment.deviceName,
                  DeviceCode: equipment.deviceCode,
                  SerialNumber: equipment.serialNumber,
                  Brand: equipment.brand,
                  Model: equipment.model,
                  Quantity: equipment.quantity,
                  Unit: equipment.unit,
                  Status: equipment.status,
                  Remark: equipment.remark,
                  EquipmentType: 1
                })
              })
              
              // 添加通用设备
              generalEquipments.forEach(equipment => {
                items.push({
                  DeviceName: equipment.deviceName,
                  DeviceCode: equipment.deviceCode,
                  SerialNumber: equipment.serialNumber,
                  Brand: equipment.brand,
                  Model: equipment.model,
                  Quantity: equipment.quantity,
                  Unit: equipment.unit,
                  Status: equipment.status,
                  Remark: equipment.remark,
                  EquipmentType: 2
                })
              })
              
              // 添加耗材
              consumables.forEach(consumable => {
                items.push({
                  DeviceName: consumable.consumableName,
                  DeviceCode: `耗材-${Date.now()}-${Math.floor(Math.random() * 1000)}`,
                  Brand: consumable.brand,
                  Model: consumable.modelSpec,
                  Quantity: consumable.quantity,
                  Unit: consumable.unit,
                  Remark: consumable.remark,
                  EquipmentType: 3
                })
              })
              
              setPreviewRecord({
                InboundNumber: values.inboundNumber,
                InboundDate: inboundDate,
                DeliveryPerson: values.deliveryPerson,
                Operator: values.operator,
                Remark: values.remark,
                Items: items
              });
              setPreviewModalVisible(true);
            });
          }}>预览</Button>,
          <Button key="cancel" onClick={closeModal}>取消</Button>,
          <Button key="ok" type="primary" onClick={saveEquipmentPurchaseInbound}>确定</Button>
        ]}
        style={{
          borderRadius: '4px'
        }}
        styles={{
          body: {
            padding: '24px'
          },
          footer: {
            padding: '16px 24px',
            display: 'flex',
            justifyContent: 'flex-end',
            gap: '12px'
          }
        }}
        width={1000}
      >
        {notification && (
          <Alert
            message={notification.message}
            type={notification.type}
            showIcon
            style={{ marginBottom: 16 }}
            action={
              <Button size="small" onClick={() => setNotification(null)}>
                关闭
              </Button>
            }
          />
        )}
        
        <Form form={form} layout="vertical">
          <div style={{ marginBottom: 20 }}>
            <div style={{ display: 'flex', gap: 16, marginBottom: 20, alignItems: 'flex-start' }}>
              <Form.Item
                name="inboundNumber"
                label="入库单号"
                rules={[{ required: true, message: '请输入入库单号' }]}
                style={{ marginBottom: 0, flex: 1 }}
              >
                <Input placeholder="请输入入库单号" disabled />
              </Form.Item>

              <Form.Item
                  name="equipmentType"
                  label="设备类型"
                  rules={[{ required: true, message: '请选择设备类型' }]}
                  style={{ marginBottom: 0, flex: 1 }}
                >
                  <Select 
                    placeholder="请选择设备类型"
                    onChange={(value) => {
                      const type = parseInt(value)
                      setEquipmentType(type)
                      // 重置设备选项
                      setEquipmentOptions([])
                      setConsumableOptions([])
                      // 重置表单
                      resetForm()
                    }}
                  >
                    <Option value={1}>专用设备</Option>
                    <Option value={2}>通用设备</Option>
                    <Option value={3}>耗材</Option>
                  </Select>
                </Form.Item>
            </div>

            <div style={{ border: '1px solid #e8e8e8', borderRadius: '4px', padding: '16px', marginBottom: 20 }}>
              <div style={{ display: 'flex', gap: 16, marginBottom: 12, alignItems: 'center' }}>
                <div style={{ width: '200px', fontWeight: 'bold' }}>设备名称</div>
                <div style={{ width: '120px', fontWeight: 'bold' }}>品牌</div>
                <div style={{ width: '120px', fontWeight: 'bold' }}>型号</div>
                <div style={{ width: '80px', fontWeight: 'bold' }}>数量</div>
                <div style={{ width: '80px', fontWeight: 'bold' }}>单位</div>
                <div style={{ width: '120px', fontWeight: 'bold' }}>设备状态</div>
                <div style={{ flex: 1 }}></div>
              </div>
              
              <div style={{ display: 'flex', gap: 16, alignItems: 'center' }}>
                {equipmentType !== 3 ? (
                  <>
                    <Form.Item
                      name="deviceName"
                      rules={[{ required: true, message: '请输入设备名称' }]}
                      style={{ marginBottom: 0, width: '200px' }}
                    >
                      <AutoComplete
                        placeholder="设备名称"
                        options={equipmentOptions}
                        onSearch={searchEquipment}
                        onSelect={handleEquipmentSelect}
                        loading={searching}
                      >
                        <Input 
                          placeholder="设备名称" 
                          onFocus={() => {
                            // 当用户点击输入框时，自动加载设备列表
                            searchEquipment('all');
                          }}
                        />
                      </AutoComplete>
                    </Form.Item>
                    <Form.Item
                      name="brand"
                      style={{ marginBottom: 0, width: '120px' }}
                    >
                      <Input placeholder="品牌" />
                    </Form.Item>
                    <Form.Item
                      name="model"
                      style={{ marginBottom: 0, width: '120px' }}
                    >
                      <Input placeholder="型号" />
                    </Form.Item>
                    <Form.Item
                      name="quantity"
                      rules={[{ required: true, message: '请输入数量' }]}
                      style={{ marginBottom: 0, width: '80px' }}
                    >
                      <InputNumber min={1} style={{ width: '100%' }} placeholder="1" />
                    </Form.Item>
                    <Form.Item
                      name="unit"
                      rules={[{ required: true, message: '请输入单位' }]}
                      style={{ marginBottom: 0, width: '80px' }}
                    >
                      <Input placeholder="台" />
                    </Form.Item>
                    <Form.Item
                      name="status"
                      rules={[{ required: true, message: '请选择设备状态' }]}
                      style={{ marginBottom: 0, width: '120px' }}
                    >
                      <Select placeholder="请选择" style={{ width: '100%' }}>
                        <Option value="正常">正常</Option>
                        <Option value="待修">待修</Option>
                        <Option value="报废">报废</Option>
                      </Select>
                    </Form.Item>
                    <div style={{ flex: 1, display: 'flex', justifyContent: 'flex-end' }}>
                      <Button type="primary" icon={<PlusOutlined />} onClick={addEquipment} />
                      <Button style={{ marginLeft: 8 }} icon={<ReloadOutlined />} onClick={resetForm} />
                    </div>
                  </>
                ) : (
                  <>
                    <Form.Item
                      name="consumableName"
                      rules={[{ required: true, message: '请输入耗材名称' }]}
                      style={{ marginBottom: 0, width: '150px' }}
                    >
                      <AutoComplete
                        placeholder="耗材名称"
                        options={consumableOptions}
                        onSearch={searchEquipment}
                        onSelect={handleEquipmentSelect}
                        loading={searching}
                      >
                        <Input 
                          placeholder="耗材名称" 
                          onFocus={() => {
                            // 当用户点击输入框时，自动加载耗材列表
                            searchEquipment('all');
                          }}
                        />
                      </AutoComplete>
                    </Form.Item>
                    <Form.Item
                      name="brand"
                      style={{ marginBottom: 0, width: '120px' }}
                    >
                      <Input placeholder="品牌" />
                    </Form.Item>
                    <Form.Item
                      name="modelSpec"
                      style={{ marginBottom: 0, width: '150px' }}
                    >
                      <Input placeholder="型号规格" />
                    </Form.Item>
                    <Form.Item
                      name="quantity"
                      rules={[{ required: true, message: '请输入数量' }]}
                      style={{ marginBottom: 0, width: '80px' }}
                    >
                      <InputNumber min={1} style={{ width: '100%' }} placeholder="1" />
                    </Form.Item>
                    <Form.Item
                      name="consumableUnit"
                      rules={[{ required: true, message: '请输入单位' }]}
                      style={{ marginBottom: 0, width: '80px' }}
                    >
                      <Input placeholder="件" />
                    </Form.Item>
                    <Form.Item
                      name="consumableRemark"
                      style={{ marginBottom: 0, width: '150px' }}
                    >
                      <Input placeholder="备注" />
                    </Form.Item>
                    <div style={{ flex: 1, display: 'flex', justifyContent: 'flex-end' }}>
                      <Button type="primary" icon={<PlusOutlined />} onClick={addEquipment} />
                      <Button style={{ marginLeft: 8 }} icon={<ReloadOutlined />} onClick={resetForm} />
                    </div>
                  </>
                )}
              </div>
            </div>

            <div style={{ marginBottom: 20 }}>
              <h3 style={{ color: '#1890ff', marginBottom: 16 }}>入库物品明细清单</h3>
              
              {specialEquipments.length === 0 && generalEquipments.length === 0 && consumables.length === 0 ? (
                <div style={{ textAlign: 'center', padding: '40px', color: '#999' }}>
                  暂无入库物品，请通过上方表单添加
                </div>
              ) : (
                <>
                  {specialEquipments.length > 0 && (
                    <div style={{ marginBottom: 16 }}>
                      <h4>已添加专用设备列表</h4>
                      <Table
                        columns={[
                          { title: '序号', dataIndex: 'index', key: 'index' },
                          { title: '设备名称', dataIndex: 'deviceName', key: 'deviceName' },
                          { title: '设备编号', dataIndex: 'deviceCode', key: 'deviceCode' },
                          { title: 'SN码', dataIndex: 'serialNumber', key: 'serialNumber', render: () => <Input placeholder="请输入SN码" /> },
                          { title: '品牌', dataIndex: 'brand', key: 'brand' },
                          { title: '型号', dataIndex: 'model', key: 'model' },
                          { title: '数量', dataIndex: 'quantity', key: 'quantity' },
                          { title: '单位', dataIndex: 'unit', key: 'unit' },
                          { title: '配件', dataIndex: 'accessories', key: 'accessories', render: () => <Input placeholder="请输入配件" /> },
                          { title: '设备状态', dataIndex: 'status', key: 'status', render: () => (
                            <Select style={{ width: '100%' }}>
                              <Option value="正常">正常</Option>
                              <Option value="待修">待修</Option>
                              <Option value="报废">报废</Option>
                            </Select>
                          ) },
                          { title: '备注', dataIndex: 'remark', key: 'remark', render: () => <Input placeholder="请输入备注" /> },
                          { 
                            title: '操作', 
                            key: 'action',
                            render: (_, record, index) => <Button danger size="small" onClick={() => removeEquipment('special', index)}>删除</Button>
                          }
                        ]}
                        dataSource={specialEquipments.map((item, index) => ({ ...item, index: index + 1 }))}
                        rowKey={(record) => `special-${record.deviceCode}`}
                        pagination={false}
                        scroll={{ x: 1200 }}
                      />
                    </div>
                  )}

                  {generalEquipments.length > 0 && (
                    <div style={{ marginBottom: 16 }}>
                      <h4>已添加通用设备列表</h4>
                      <Table
                        columns={[
                          { title: '序号', dataIndex: 'index', key: 'index' },
                          { title: '设备名称', dataIndex: 'deviceName', key: 'deviceName' },
                          { title: '设备编号', dataIndex: 'deviceCode', key: 'deviceCode' },
                          { title: 'SN码', dataIndex: 'serialNumber', key: 'serialNumber', render: () => <Input placeholder="请输入SN码" /> },
                          { title: '品牌', dataIndex: 'brand', key: 'brand' },
                          { title: '型号', dataIndex: 'model', key: 'model' },
                          { title: '数量', dataIndex: 'quantity', key: 'quantity' },
                          { title: '单位', dataIndex: 'unit', key: 'unit' },
                          { title: '配件', dataIndex: 'accessories', key: 'accessories', render: () => <Input placeholder="请输入配件" /> },
                          { title: '设备状态', dataIndex: 'status', key: 'status', render: () => (
                            <Select style={{ width: '100%' }}>
                              <Option value="正常">正常</Option>
                              <Option value="待修">待修</Option>
                              <Option value="报废">报废</Option>
                            </Select>
                          ) },
                          { title: '备注', dataIndex: 'remark', key: 'remark', render: () => <Input placeholder="请输入备注" /> },
                          { 
                            title: '操作', 
                            key: 'action',
                            render: (_, record, index) => <Button danger size="small" onClick={() => removeEquipment('general', index)}>删除</Button>
                          }
                        ]}
                        dataSource={generalEquipments.map((item, index) => ({ ...item, index: index + 1 }))}
                        rowKey={(record) => `general-${record.deviceCode}`}
                        pagination={false}
                        scroll={{ x: 1200 }}
                      />
                    </div>
                  )}

                  {consumables.length > 0 && (
                    <div style={{ marginBottom: 16 }}>
                      <h4>已添加耗材列表</h4>
                      <Table
                        columns={[
                          { title: '序号', dataIndex: 'index', key: 'index' },
                          { title: '耗材名称', dataIndex: 'consumableName', key: 'consumableName' },
                          { title: '品牌', dataIndex: 'brand', key: 'brand' },
                          { title: '型号规格', dataIndex: 'modelSpec', key: 'modelSpec' },
                          { title: '数量', dataIndex: 'quantity', key: 'quantity' },
                          { title: '单位', dataIndex: 'unit', key: 'unit' },
                          { title: '备注', dataIndex: 'remark', key: 'remark' },
                          { 
                            title: '操作', 
                            key: 'action',
                            render: (_, record, index) => <Button danger size="small" onClick={() => removeEquipment('consumable', index)}>删除</Button>
                          }
                        ]}
                        dataSource={consumables.map((item, index) => ({ ...item, index: index + 1 }))}
                        rowKey="index"
                        pagination={false}
                        scroll={{ x: 800 }}
                      />
                    </div>
                  )}
                </>
              )}
            </div>

            <div style={{ marginBottom: 20 }}>
              <h3>其他信息</h3>
              <div style={{ display: 'flex', gap: 16, marginBottom: 16 }}>
                <Form.Item
                  name="deliveryPerson"
                  label="交货人"
                  style={{ marginBottom: 0, flex: 1 }}
                >
                  <Input placeholder="请输入交货人" />
                </Form.Item>

                <Form.Item
                  name="operator"
                  label="操作人"
                  style={{ marginBottom: 0, flex: 1 }}
                >
                  <Input placeholder="请输入操作人" />
                </Form.Item>

                <Form.Item
                  name="inboundDate"
                  label="*入库日期"
                  rules={[{ required: true, message: '请选择入库日期' }]}
                  style={{ marginBottom: 0, flex: 1 }}
                >
                  <DatePicker style={{ width: '100%' }} />
                </Form.Item>
              </div>
            </div>

            <Form.Item
              name="remark"
              label="入库单备注"
            >
              <TextArea rows={4} placeholder="请输入入库单备注（可选）" />
            </Form.Item>
          </div>
        </Form>
      </Modal>

      {/* 设备采购入库预览模态框 */}
      <Modal
        title="设备采购入库单预览"
        open={previewModalVisible}
        onCancel={closePreviewModal}
        footer={[
          <Button key="print" type="default">打印</Button>,
          <Button key="export" type="default">保存PDF</Button>,
          <Button key="close" type="primary" onClick={closePreviewModal}>关闭预览</Button>
        ]}
        width={1000}
      >
        {previewRecord && (
          <div style={{ padding: 20 }}>
            <h2 style={{ textAlign: 'center', marginBottom: 30 }}>设备采购入库单</h2>
            <div style={{ marginBottom: 20 }}>
              <div style={{ marginBottom: 10 }}>
                <strong>入库单号：</strong>{previewRecord.InboundNumber}
              </div>
            </div>
            
            {/* 专用设备明细 */}
            <div style={{ marginBottom: 20 }}>
              <h3>专用设备明细</h3>
              <table style={{ width: '100%', borderCollapse: 'collapse', marginTop: 10 }}>
                <thead>
                  <tr style={{ borderBottom: '1px solid #ddd' }}>
                    <th style={{ padding: '8px', textAlign: 'left' }}>序号</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>设备名称</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>设备编号</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>SN码</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>品牌</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>型号</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>数量</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>单位</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>设备状态</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>备注</th>
                  </tr>
                </thead>
                <tbody>
                  {previewRecord.Items.filter(item => item.EquipmentType === 1).map((item, index) => (
                    <tr key={index} style={{ borderBottom: '1px solid #f0f0f0' }}>
                      <td style={{ padding: '8px' }}>{index + 1}</td>
                      <td style={{ padding: '8px' }}>{item.DeviceName || '-'}</td>
                      <td style={{ padding: '8px' }}>{item.DeviceCode || '-'}</td>
                      <td style={{ padding: '8px' }}>{item.SerialNumber || '-'}</td>
                      <td style={{ padding: '8px' }}>{item.Brand || '-'}</td>
                      <td style={{ padding: '8px' }}>{item.Model || '-'}</td>
                      <td style={{ padding: '8px' }}>{item.Quantity || 0}</td>
                      <td style={{ padding: '8px' }}>{item.Unit || '-'}</td>
                      <td style={{ padding: '8px' }}>{item.Status || '-'}</td>
                      <td style={{ padding: '8px' }}>{item.Remark || '-'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            
            {/* 通用设备明细 */}
            <div style={{ marginBottom: 20 }}>
              <h3>通用设备明细</h3>
              <table style={{ width: '100%', borderCollapse: 'collapse', marginTop: 10 }}>
                <thead>
                  <tr style={{ borderBottom: '1px solid #ddd' }}>
                    <th style={{ padding: '8px', textAlign: 'left' }}>序号</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>设备名称</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>设备编号</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>SN码</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>品牌</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>型号</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>数量</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>单位</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>设备状态</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>备注</th>
                  </tr>
                </thead>
                <tbody>
                  {previewRecord.Items.filter(item => item.EquipmentType === 2).map((item, index) => (
                    <tr key={index} style={{ borderBottom: '1px solid #f0f0f0' }}>
                      <td style={{ padding: '8px' }}>{index + 1}</td>
                      <td style={{ padding: '8px' }}>{item.DeviceName || '-'}</td>
                      <td style={{ padding: '8px' }}>{item.DeviceCode || '-'}</td>
                      <td style={{ padding: '8px' }}>{item.SerialNumber || '-'}</td>
                      <td style={{ padding: '8px' }}>{item.Brand || '-'}</td>
                      <td style={{ padding: '8px' }}>{item.Model || '-'}</td>
                      <td style={{ padding: '8px' }}>{item.Quantity || 0}</td>
                      <td style={{ padding: '8px' }}>{item.Unit || '-'}</td>
                      <td style={{ padding: '8px' }}>{item.Status || '-'}</td>
                      <td style={{ padding: '8px' }}>{item.Remark || '-'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            
            {/* 耗材明细 */}
            <div style={{ marginBottom: 20 }}>
              <h3>耗材明细</h3>
              <table style={{ width: '100%', borderCollapse: 'collapse', marginTop: 10 }}>
                <thead>
                  <tr style={{ borderBottom: '1px solid #ddd' }}>
                    <th style={{ padding: '8px', textAlign: 'left' }}>序号</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>耗材名称</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>品牌</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>型号规格</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>数量</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>单位</th>
                    <th style={{ padding: '8px', textAlign: 'left' }}>备注</th>
                  </tr>
                </thead>
                <tbody>
                  {previewRecord.Items.filter(item => item.EquipmentType === 3).map((item, index) => (
                    <tr key={index} style={{ borderBottom: '1px solid #f0f0f0' }}>
                      <td style={{ padding: '8px' }}>{index + 1}</td>
                      <td style={{ padding: '8px' }}>{item.DeviceName || '-'}</td>
                      <td style={{ padding: '8px' }}>{item.Brand || '-'}</td>
                      <td style={{ padding: '8px' }}>{item.Model || '-'}</td>
                      <td style={{ padding: '8px' }}>{item.Quantity || 0}</td>
                      <td style={{ padding: '8px' }}>{item.Unit || '-'}</td>
                      <td style={{ padding: '8px' }}>{item.Remark || '-'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            
            <div style={{ marginBottom: 10 }}>
              <strong>交货人：</strong>{previewRecord.DeliveryPerson || '-'}
            </div>
            <div style={{ marginBottom: 10 }}>
              <strong>操作人：</strong>{previewRecord.Operator || '-'}
            </div>
            <div style={{ marginBottom: 10 }}>
              <strong>入库日期：</strong>{previewRecord.InboundDate}
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

export default EquipmentPurchaseInbound