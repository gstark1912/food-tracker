import { setActivePinia, createPinia } from 'pinia'
import { useTrackerStore } from '../trackerStore.js'

function mockFetchResponse(data) {
    return Promise.resolve({
        ok: true,
        json: () => Promise.resolve(data),
    })
}

function mockFetchError() {
    return Promise.resolve({
        ok: false,
        json: () => Promise.resolve({ error: 'Server error' }),
    })
}

describe('trackerStore - daily entries', () => {
    let store

    beforeEach(() => {
        setActivePinia(createPinia())
        store = useTrackerStore()
    })

    afterEach(() => {
        vi.restoreAllMocks()
    })

    /**
     * **Validates: Requirements 4.4, 6.1, 6.2, 6.3** (Property 6)
     */
    describe('Property 6: Store accumulates items correctly in pagination', () => {
        it('page 1 replaces the list', async () => {
            const page1Items = [
                { date: '2026-03-27', foodMañana: 3, foodMediodia: 5, foodTarde: 2, foodNoche: 1, totalExercise: 8 },
            ]
            vi.spyOn(globalThis, 'fetch').mockImplementation(() =>
                mockFetchResponse({ items: page1Items, totalCount: 5, hasMore: true })
            )

            await store.loadDailyEntries(1)

            expect(store.dailyEntries).toEqual(page1Items)
            expect(store.dailyEntriesHasMore).toBe(true)
            expect(store.dailyEntriesPage).toBe(1)
        })

        it('page > 1 concatenates items to existing list', async () => {
            const page1Items = [
                { date: '2026-03-27', foodMañana: 3, foodMediodia: 5, foodTarde: 2, foodNoche: 1, totalExercise: 8 },
            ]
            const page2Items = [
                { date: '2026-03-26', foodMañana: 2, foodMediodia: 3, foodTarde: 1, foodNoche: 2, totalExercise: 5 },
            ]

            vi.spyOn(globalThis, 'fetch')
                .mockImplementationOnce(() => mockFetchResponse({ items: page1Items, totalCount: 5, hasMore: true }))
                .mockImplementationOnce(() => mockFetchResponse({ items: page2Items, totalCount: 5, hasMore: false }))

            await store.loadDailyEntries(1)
            await store.loadDailyEntries(2)

            expect(store.dailyEntries).toEqual([...page1Items, ...page2Items])
            expect(store.dailyEntriesHasMore).toBe(false)
            expect(store.dailyEntriesPage).toBe(2)
        })

        it('dailyEntriesHasMore is updated correctly on each load', async () => {
            vi.spyOn(globalThis, 'fetch')
                .mockImplementationOnce(() =>
                    mockFetchResponse({ items: [{ date: '2026-03-27' }], totalCount: 2, hasMore: true })
                )
                .mockImplementationOnce(() =>
                    mockFetchResponse({ items: [{ date: '2026-03-26' }], totalCount: 2, hasMore: false })
                )

            await store.loadDailyEntries(1)
            expect(store.dailyEntriesHasMore).toBe(true)

            await store.loadDailyEntries(2)
            expect(store.dailyEntriesHasMore).toBe(false)
        })
    })

    /**
     * **Validates: Requirement 5.3** (Property 7)
     */
    describe('Property 7: Error on subsequent load preserves existing items', () => {
        it('error on page 2 preserves items from page 1', async () => {
            const page1Items = [
                { date: '2026-03-27', foodMañana: 3, foodMediodia: 5, foodTarde: 2, foodNoche: 1, totalExercise: 8 },
            ]

            vi.spyOn(globalThis, 'fetch')
                .mockImplementationOnce(() => mockFetchResponse({ items: page1Items, totalCount: 5, hasMore: true }))
                .mockImplementationOnce(() => mockFetchError())

            await store.loadDailyEntries(1)
            expect(store.dailyEntries).toEqual(page1Items)

            await store.loadDailyEntries(2)
            expect(store.dailyEntries).toEqual(page1Items)
            expect(store.dailyEntriesError).toBeTruthy()
        })
    })
})
