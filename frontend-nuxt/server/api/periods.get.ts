export default defineEventHandler(async (event) => {
  try {
    return await callApi<ReportingPeriod[]>(event, '/api/reportingperiods')
  } catch (err) {
    throw apiError(err)
  }
})
