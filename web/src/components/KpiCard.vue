<template>
    <div class="kpi-card">
        <span class="kpi-label">{{ label }}</span>
        <span class="kpi-value">{{ value }}</span>
        <span v-if="previousValue !== null" :class="['kpi-comparison', directionClass]">
            {{ arrow }} {{ absDiff }}
        </span>
    </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
    label: { type: String, required: true },
    value: { type: Number, required: true },
    previousValue: { type: Number, default: null },
})

const diff = computed(() => props.value - props.previousValue)

const direction = computed(() => {
    if (props.previousValue === null) return null
    if (props.value > props.previousValue) return 'sube'
    if (props.value < props.previousValue) return 'baja'
    return 'igual'
})

const absDiff = computed(() => Math.abs(diff.value))

const arrow = computed(() => {
    if (direction.value === 'sube') return '▲'
    if (direction.value === 'baja') return '▼'
    return '●'
})

const directionClass = computed(() => {
    if (direction.value === 'sube') return 'comparison-up'
    if (direction.value === 'baja') return 'comparison-down'
    return 'comparison-equal'
})
</script>

<style scoped>
.kpi-card {
    background: #fff;
    border-radius: 0.75rem;
    padding: 1rem;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.07);
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 0.25rem;
    min-width: 0;
    flex: 1;
}

.kpi-label {
    font-size: 0.7rem;
    color: #888;
    text-transform: uppercase;
    letter-spacing: 0.05em;
}

.kpi-value {
    font-size: 1.6rem;
    font-weight: 700;
}

.kpi-comparison {
    padding: 0.2rem 0.6rem;
    border-radius: 1rem;
    font-size: 0.75rem;
    font-weight: 600;
}

.comparison-up {
    background: #e8f5e9;
    color: #2e7d32;
}

.comparison-down {
    background: #ffebee;
    color: #c62828;
}

.comparison-equal {
    background: #f5f5f5;
    color: #888;
}
</style>
