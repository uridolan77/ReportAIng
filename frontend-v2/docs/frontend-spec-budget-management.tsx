// features/cost-management/components/BudgetManagement.tsx
export const BudgetManagement: React.FC = () => {
  const { data: budgets } = useGetBudgetsQuery()
  const [createBudget] = useCreateBudgetMutation()
  const [updateBudget] = useUpdateBudgetMutation()
  const [deleteBudget] = useDeleteBudgetMutation()

  const [open, setOpen] = useState(false)
  const [editingBudget, setEditingBudget] = useState<BudgetManagement | null>(null)

  const handleSubmit = async (budgetData: CreateBudgetRequest) => {
    try {
      if (editingBudget) {
        await updateBudget({ 
          budgetId: editingBudget.id, 
          budget: budgetData 
        }).unwrap()
      } else {
        await createBudget(budgetData).unwrap()
      }
      setOpen(false)
      setEditingBudget(null)
    } catch (error) {
      // Handle error
    }
  }

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" mb={2}>
        <Typography variant="h5">Budget Management</Typography>
        <Button 
          variant="contained" 
          onClick={() => setOpen(true)}
          startIcon={<AddIcon />}
        >
          Create Budget
        </Button>
      </Box>

      <Grid container spacing={2}>
        {budgets?.budgets.map((budget) => (
          <Grid item xs={12} md={6} lg={4} key={budget.id}>
            <BudgetCard 
              budget={budget}
              onEdit={() => {
                setEditingBudget(budget)
                setOpen(true)
              }}
              onDelete={() => deleteBudget(budget.id)}
            />
          </Grid>
        ))}
      </Grid>

      <BudgetDialog
        open={open}
        budget={editingBudget}
        onClose={() => {
          setOpen(false)
          setEditingBudget(null)
        }}
        onSubmit={handleSubmit}
      />
    </Box>
  )
}
