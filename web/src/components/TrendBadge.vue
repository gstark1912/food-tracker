<template>
  <span v-if="trend" :class="['trend-badge', trend === 'down' ? 'trend-down' : 'trend-up']">
    {{ trend === 'down' ? '↓ Mejor' : '↑ Peor' }}
  </span>
</template>

<script setup>
defineProps({
  trend: { type: String, default: null }, // 'up' | 'down' | null
})
</script>

<style scoped>
.trend-badge {
  padding: 0.2rem 0.6rem;
  border-radius: 1rem;
  font-size: 0.75rem;
  font-weight: 600;
}
.trend-down {
  background: #e8f5e9;
  color: #2e7d32;
}
.trend-up {
  background: #ffebee;
  color: #c62828;
}
</style>
