"use client";

import { Button } from "@/components/ui/Button";
import { Card } from "@/components/ui/Card";
import { Modal } from "@/components/ui/Modal";
import { PlusCircle, Trash } from "lucide-react";
import { useState } from "react";

interface Shortcut {
  label: string;
  description: string;
  href: string;
  icon: React.ElementType;
  color: string;
}

interface ShortcutManagerProps {
  shortcuts: Shortcut[];
  onAddShortcut: (shortcut: Shortcut) => void;
  onRemoveShortcut: (href: string) => void;
}

export function ShortcutManager({ shortcuts, onAddShortcut, onRemoveShortcut }: ShortcutManagerProps) {
  const [isOpen, setIsOpen] = useState(false);

  const toggleModal = () => setIsOpen(!isOpen);

  return (
    <>
      <Button onClick={toggleModal} variant="primary" className="flex items-center gap-2">
        <PlusCircle size={18} /> Manage Shortcuts
      </Button>

      <Modal isOpen={isOpen} onClose={toggleModal} title="Manage Shortcuts">
        <div className="space-y-4">
          {shortcuts.map((shortcut) => {
            const Icon = shortcut.icon;
            return (
              <Card key={shortcut.href} className="flex items-center justify-between p-4">
                <div className="flex items-center gap-4">
                  <div
                    className={`w-12 h-12 rounded-xl bg-gradient-to-br ${shortcut.color} flex items-center justify-center shadow-md`}
                  >
                    <Icon size={24} className="text-white" />
                  </div>
                  <div>
                    <h3 className="text-base font-semibold text-[var(--text-primary)]">
                      {shortcut.label}
                    </h3>
                    <p className="text-xs text-[var(--text-tertiary)] mt-1">
                      {shortcut.description}
                    </p>
                  </div>
                </div>
                <Button
                  onClick={() => onRemoveShortcut(shortcut.href)}
                  variant="danger"
                  className="flex items-center gap-2"
                >
                  <Trash size={18} /> Remove
                </Button>
              </Card>
            );
          })}

          <Button onClick={() => onAddShortcut({
            label: "New Shortcut",
            description: "Description for new shortcut",
            href: "/new",
            icon: PlusCircle,
            color: "from-gray-500 to-gray-700",
          })} variant="success" className="flex items-center gap-2">
            <PlusCircle size={18} /> Add Shortcut
          </Button>
        </div>
      </Modal>
    </>
  );
}
