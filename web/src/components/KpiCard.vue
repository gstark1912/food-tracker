<template>
    <div class="kpi-card">
        <span class="kpi-label">{{ label }}</span>
        <div class="kpi-values">
            <span class="kpi-current">{{ value }}</span>
            <span v-if="previousValue !== null" class="kpi-separator">←</span>
            <span v-if="previousValue !== null" class="kpi-previous">{{ previousValue }}</span>
        </div>
        <div v-if="previousValue !== null" class="kpi-labels-row">
            <span class="kpi-sublabel">actual</span>
            <span v-if="previousValue !== null" class="kpi-sublabel">anterior</span>
        </div>
        <span v-if="previousValue !== null" :class="['kpi-comparison', sentimentClass]">
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
    lowerIsBetter: { type: Boolean, default: false },
})

const diff = computed(() => props.value - props.previousValue)
const absDiff = computed(() => {
    const val = Math.abs(diff.value)
    return Number.isInteger(val) ? val : val.toFixed(1)
})

const direction = computed(() => {
    if (props.previousValue === null) return null
    if (diff.value > 0) return 'up'
    if (diff.value < 0) return 'down'
    return 'equal'
})

const arrow = computed(() => {
    if (direction.value === 'up') return '▲'
    if (direction.value === 'down') return '▼'
    return '●'
})

const sentimentClass = computed(() => {
    if (direction.value === 'equal') return 'sentiment-neutral'
    const isGood = props.lowerIsBetter
        ? direction.value === 'down'
        : direction.value === 'up'
    return isGood ? 'sentiment-good' : 'sentiment-bad'
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
    gap: 0.15rem;
    min-width: 0;
    flex: 1;
}

.kpi-label {
    font-size: 0.7rem;
    color: #888;
    text-transform: uppercase;
    letter-spacing: 0.05em;
}

.kpi-values {
    display: flex;
    align-items: baseline;
    gap: 0.4rem;
}

.kpi-current {
    font-size: 1.6rem;
    font-weight: 700;
}

.kpi-separator {
    font-size: 0.85rem;
    color: #bbb;
}

.kpi-previous {
    font-size: 1rem;
    color: #999;
    font-weight: 500;
}

.kpi-labels-row {
    display: flex;
    justify-content: space-between;
    width: 100%;
    padding: 0 0.5rem;
}

.kpi-sublabel {
    font-size: 0.6rem;
    color: #aaa;
    text-transform: lowercase;
}

.kpi-comparison {
    padding: 0.2rem 0.6rem;
    border-radius: 1rem;
    font-size: 0.75rem;
    font-weight: 600;
    margin-top: 0.15rem;
}

.sentiment-good {
    background: #e8f5e9;
    color: #2e7d32;
}

.sentiment-bad {
    background: #ffebee;
    color: #c62828;
}

.sentiment-neutral {
    background: #f5f5f5;
    color: #888;
}
</style>
