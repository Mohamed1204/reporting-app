export default defineEventHandler(async (event) => {
  const id = Number(getRouterParam(event, 'id'))

  // Rejected here, not upstream: a junk id is our bad request, not .NET's.
  if (!Number.isInteger(id)) {
    throw createError({ statusCode: 400, statusMessage: 'Period id must be an integer' })
  }

  try {
    return await callApi<ReportingPeriod>(event, `/api/reportingperiods/${id}`)
  } catch (err) {
    throw apiError(err, { 404: 'Period not found' })
  }
})
