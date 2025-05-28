import { StateCreator } from 'zustand';

// Logging middleware
export const logger = <T>(
  f: StateCreator<T, [], [], T>,
  name?: string
): StateCreator<T, [], [], T> => (set, get, store) => {
  const loggedSet: typeof set = (...args) => {
    console.group(`🔄 ${name || 'Store'} Update`);
    console.log('Previous State:', get());
    set(...args);
    console.log('New State:', get());
    console.groupEnd();
  };
  
  store.setState = loggedSet;
  return f(loggedSet, get, store);
};

// Performance monitoring middleware
export const performanceMonitor = <T>(
  f: StateCreator<T, [], [], T>,
  name?: string
): StateCreator<T, [], [], T> => (set, get, store) => {
  const monitoredSet: typeof set = (...args) => {
    const start = performance.now();
    set(...args);
    const end = performance.now();
    
    const duration = end - start;
    if (duration > 5) { // Log if update takes more than 5ms
      console.warn(`⚠️ ${name || 'Store'} update took ${duration.toFixed(2)}ms`);
    }
  };
  
  store.setState = monitoredSet;
  return f(monitoredSet, get, store);
};

// Validation middleware
export const validator = <T>(
  f: StateCreator<T, [], [], T>,
  validationRules: Record<string, (value: any) => boolean | string>
): StateCreator<T, [], [], T> => (set, get, store) => {
  const validatedSet: typeof set = (partial, replace) => {
    if (typeof partial === 'function') {
      const newState = partial(get());
      
      // Validate the new state
      for (const [key, rule] of Object.entries(validationRules)) {
        if (key in newState) {
          const result = rule((newState as any)[key]);
          if (result !== true) {
            console.error(`Validation failed for ${key}: ${result}`);
            return; // Don't update state if validation fails
          }
        }
      }
    }
    
    set(partial, replace);
  };
  
  store.setState = validatedSet;
  return f(validatedSet, get, store);
};

// Undo/Redo middleware
export interface UndoRedoState<T> {
  past: T[];
  present: T;
  future: T[];
  undo: () => void;
  redo: () => void;
  canUndo: boolean;
  canRedo: boolean;
  clearHistory: () => void;
}

export const undoRedo = <T>(
  f: StateCreator<T, [], [], T>,
  maxHistorySize: number = 50
): StateCreator<UndoRedoState<T>, [], [], UndoRedoState<T>> => (set, get, store) => {
  const initialState = f(
    (partial, replace) => {
      const currentState = get();
      const newPresent = typeof partial === 'function' 
        ? partial(currentState.present) 
        : { ...currentState.present, ...partial };
      
      set({
        past: [...currentState.past, currentState.present].slice(-maxHistorySize),
        present: newPresent,
        future: [], // Clear future when new action is performed
        canUndo: true,
        canRedo: false
      });
    },
    () => get().present,
    {
      ...store,
      setState: (partial, replace) => {
        const currentState = get();
        const newPresent = typeof partial === 'function' 
          ? partial(currentState.present) 
          : { ...currentState.present, ...partial };
        
        set({
          past: [...currentState.past, currentState.present].slice(-maxHistorySize),
          present: newPresent,
          future: [],
          canUndo: true,
          canRedo: false
        });
      }
    }
  );

  return {
    past: [],
    present: initialState,
    future: [],
    canUndo: false,
    canRedo: false,
    
    undo: () => {
      const { past, present, future } = get();
      if (past.length === 0) return;
      
      const previous = past[past.length - 1];
      const newPast = past.slice(0, past.length - 1);
      
      set({
        past: newPast,
        present: previous,
        future: [present, ...future],
        canUndo: newPast.length > 0,
        canRedo: true
      });
    },
    
    redo: () => {
      const { past, present, future } = get();
      if (future.length === 0) return;
      
      const next = future[0];
      const newFuture = future.slice(1);
      
      set({
        past: [...past, present],
        present: next,
        future: newFuture,
        canUndo: true,
        canRedo: newFuture.length > 0
      });
    },
    
    clearHistory: () => {
      set({
        past: [],
        future: [],
        canUndo: false,
        canRedo: false
      });
    }
  };
};

// Optimistic updates middleware
export const optimisticUpdates = <T>(
  f: StateCreator<T, [], [], T>
): StateCreator<T & { rollback: () => void }, [], [], T & { rollback: () => void }> => (set, get, store) => {
  let snapshot: T | null = null;
  
  const optimisticSet: typeof set = (partial, replace) => {
    // Save snapshot before optimistic update
    if (!snapshot) {
      snapshot = { ...get() } as T;
    }
    
    set(partial, replace);
  };
  
  const baseState = f(optimisticSet, get, store);
  
  return {
    ...baseState,
    rollback: () => {
      if (snapshot) {
        set(snapshot as any, true);
        snapshot = null;
      }
    }
  };
};

// Debounced updates middleware
export const debouncedUpdates = <T>(
  f: StateCreator<T, [], [], T>,
  delay: number = 300
): StateCreator<T, [], [], T> => (set, get, store) => {
  let timeoutId: NodeJS.Timeout | null = null;
  let pendingUpdate: Parameters<typeof set> | null = null;
  
  const debouncedSet: typeof set = (...args) => {
    pendingUpdate = args;
    
    if (timeoutId) {
      clearTimeout(timeoutId);
    }
    
    timeoutId = setTimeout(() => {
      if (pendingUpdate) {
        set(...pendingUpdate);
        pendingUpdate = null;
        timeoutId = null;
      }
    }, delay);
  };
  
  store.setState = debouncedSet;
  return f(debouncedSet, get, store);
};

// Computed properties middleware
export const computed = <T, C>(
  f: StateCreator<T, [], [], T>,
  computedProps: Record<keyof C, (state: T) => C[keyof C]>
): StateCreator<T & C, [], [], T & C> => (set, get, store) => {
  const baseState = f(set, get, store);
  
  // Create computed properties
  const computedValues = {} as C;
  for (const [key, computeFn] of Object.entries(computedProps)) {
    Object.defineProperty(computedValues, key, {
      get: () => computeFn(get() as T),
      enumerable: true,
      configurable: true
    });
  }
  
  return {
    ...baseState,
    ...computedValues
  };
};

// Batch updates middleware
export const batchUpdates = <T>(
  f: StateCreator<T, [], [], T>
): StateCreator<T & { batch: (updates: Array<Partial<T>>) => void }, [], [], T & { batch: (updates: Array<Partial<T>>) => void }> => (set, get, store) => {
  const baseState = f(set, get, store);
  
  return {
    ...baseState,
    batch: (updates: Array<Partial<T>>) => {
      const currentState = get();
      const batchedUpdate = updates.reduce((acc, update) => ({ ...acc, ...update }), currentState);
      set(batchedUpdate as any, true);
    }
  };
};

// Error boundary middleware
export const errorBoundary = <T>(
  f: StateCreator<T, [], [], T>,
  onError?: (error: Error, state: T) => void
): StateCreator<T, [], [], T> => (set, get, store) => {
  const safeSet: typeof set = (...args) => {
    try {
      set(...args);
    } catch (error) {
      console.error('State update error:', error);
      onError?.(error as Error, get());
    }
  };
  
  store.setState = safeSet;
  return f(safeSet, get, store);
};
