/**
 * Home Page - Minimal Query Interface
 *
 * Clean, minimal home page showing only the chat box area without tabs or headers.
 * Provides a focused query experience similar to ChatGPT interface.
 */

import React from 'react';
import { QueryInterface } from '../components/QueryInterface/QueryInterface';

const QueryPage: React.FC = () => {
  return (
    <div style={{
      padding: 0,
      margin: 0,
      background: 'transparent'
    }}>
      {/* Minimal Query Interface - Only chat box area, no tabs or titles */}
      <QueryInterface />
    </div>
  );
};

export default QueryPage;
