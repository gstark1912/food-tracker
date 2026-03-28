<template>
  <div class="fib-control">
    <button
      class="fib-btn"
      :disabled="disabled || value === 0"
      @click="$emit('decrement')"
      aria-label="Decrementar"
    >−</button>
    <span class="fib-value">{{ value }}</span>
    <button
      class="fib-btn"
      :disabled="disabled || value === 13"
      @click="$emit('increment')"
      aria-label="Incrementar"
    >+</button>
  </div>
</template>

<script setup>
defineProps({
  value: { type: Number, required: true },
  disabled: { type: Boolean, default: false },
})
defineEmits(['increment', 'decrement'])
</script>

<style scoped>
.fib-control {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}
.fib-btn {
  width: 2.25rem;
  height: 2.25rem;
  border: 1px solid #ddd;
  border-radius: 50%;
  background: #fff;
  font-size: 1.25rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background 0.15s;
  -webkit-tap-highlight-color: transparent;
}
.fib-btn:disabled {
  opacity: 0.3;
  cursor: not-allowed;
}
.fib-btn:not(:disabled):active {
  background: #f0f0f0;
}
.fib-value {
  min-width: 1.5rem;
  text-align: center;
  font-size: 1.1rem;
  font-weight: 600;
}
</style>
