<template>
  <div class="app">
    <h1>设备管理系统</h1>
    <RepairEquipmentManagement />
  </div>
</template>

<script>
import RepairEquipmentManagement from './components/RepairEquipmentManagement.vue'

export default {
  name: 'App',
  components: {
    RepairEquipmentManagement
  }
}
</script>

<style>
.app {
  padding: 20px;
}

h1 {
  margin-bottom: 20px;
  color: #303133;
}
</style>