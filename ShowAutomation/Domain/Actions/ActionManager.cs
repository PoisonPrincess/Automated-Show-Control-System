using System;
using System.Collections.Generic;

namespace ShowAutomation.Domain.Actions
{
    class ActionManager
    {
        private Stack<Action> undoStack = new Stack<Action>();
        private Stack<Action> redoStack = new Stack<Action>();
        private bool changesMade = false;

        public bool ChangesMade() {
            return changesMade;
        }

        public void Save() {
            changesMade = false;
        }

        public void UndoLastAction() {
            Action undo = undoStack.Pop();
            undo.Undo();
            redoStack.Push(undo);
            UpdateMenuItems();
            changesMade = CanUndo();
        }

        public void RedoLastAction() {
            Action redo = redoStack.Pop();
            redo.Perform();
            undoStack.Push(redo);
            UpdateMenuItems();
            changesMade = true;
        }

        public void PerformAction(Action action) {
            action.Perform();
            undoStack.Push(action);
            redoStack.Clear();
            UpdateMenuItems();
            changesMade = true;
        }

        private void UpdateMenuItems() {
            MainWindow.main.UndoButton.IsEnabled = CanUndo();
            MainWindow.main.RedoButton.IsEnabled = CanRedo();
            MainWindow.main.UndoButton.Header = CanUndo() ? "Undo " + UndoTitle() : "Undo";
            MainWindow.main.RedoButton.Header = CanRedo() ? "Redo " + RedoTitle() : "Redo";
        }

        public bool CanRedo() {
            return redoStack.Count > 0;
        }

        private String RedoTitle() {
            return redoStack.Peek().Title();
        }

        public bool CanUndo()
        {
            return undoStack.Count > 0;
        }

        private String UndoTitle()
        {
            return undoStack.Peek().Title();
        }
    }
}
