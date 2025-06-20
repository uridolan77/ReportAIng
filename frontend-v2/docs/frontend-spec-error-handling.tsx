// shared/components/ErrorBoundary.tsx
export const ErrorBoundary: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  return (
    <ErrorBoundaryComponent
      FallbackComponent={({ error, resetErrorBoundary }) => (
        <Alert severity="error">
          <AlertTitle>Something went wrong</AlertTitle>
          {error.message}
          <Button onClick={resetErrorBoundary}>Try again</Button>
        </Alert>
      )}
    >
      {children}
    </ErrorBoundaryComponent>
  )
}

// shared/components/LoadingStates.tsx
export const MetricsLoading: React.FC = () => (
  <Grid container spacing={2}>
    {[1, 2, 3, 4].map((i) => (
      <Grid item xs={12} md={3} key={i}>
        <Skeleton variant="rectangular" height={120} />
      </Grid>
    ))}
  </Grid>
)

export const ChartLoading: React.FC = () => (
  <Skeleton variant="rectangular" height={300} />
)
