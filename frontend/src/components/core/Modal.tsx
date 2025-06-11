/**
 * Modern Modal and Overlay Components
 * 
 * Provides comprehensive modal, drawer, popover, and tooltip components.
 */

import React, { forwardRef } from 'react';
import { Modal as AntModal, Drawer as AntDrawer, Popover as AntPopover, Tooltip as AntTooltip } from 'antd';
import { designTokens } from './design-system';

// Basic exports - these will be expanded later
export const Modal = forwardRef<any, any>((props, ref) => <AntModal ref={ref} {...props} />);
export const Drawer = forwardRef<any, any>((props, ref) => <AntDrawer ref={ref} {...props} />);
export const Popover = forwardRef<any, any>((props, ref) => <AntPopover ref={ref} {...props} />);
export const Tooltip = forwardRef<any, any>((props, ref) => <AntTooltip ref={ref} {...props} />);

export const Dialog = Modal;
export const ConfirmDialog = Modal;
