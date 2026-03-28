import { createRouter, createWebHistory } from 'vue-router'
import TrackerView from '../views/TrackerView.vue'
import StatisticsView from '../views/StatisticsView.vue'

const routes = [
    { path: '/', component: TrackerView },
    { path: '/stats', component: StatisticsView },
]

export default createRouter({
    history: createWebHistory(),
    routes,
})
